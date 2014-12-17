#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using jabber;
using jabber.connection;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.Replays;
using LegendaryClient.Logic.SoundLogic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using Microsoft.Win32;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Reroll.Pojo;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;
using PVPNetConnect.RiotObjects.Platform.Trade;
using Color = System.Drawing.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using ListViewItem = System.Windows.Controls.ListViewItem;
using Message = jabber.protocol.client.Message;
using Timer = System.Windows.Forms.Timer;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ChampSelectPage.xaml
    /// </summary>
    public partial class ChampSelectPage
    {
        internal static object LobbyContent = new object();
        private readonly List<string> PreviousPlayers = new List<string>(); //Needs to be initialized!
        private readonly Page previousPage;

        private bool AreWePurpleSide;
        private PotentialTradersDTO CanTradeWith;
        private List<ChampionDTO> ChampList;
        private List<ChampionBanInfoDTO> ChampionsForBan;
        private Room Chatroom;
        private Timer CountdownTimer;
        private bool HasLaunchedGame;
        private bool HasLockedIn;
        private GameDTO LatestDto;
        private MasteryBookDTO MyMasteries;
        private SpellBookDTO MyRunes;
        private bool QuickLoad; //Don't load masteries and runes on load at start
        private bool _BanningPhase;
        private int _LastPickTurn;
        private double _MyChampId;
        private GameTypeConfigDTO configType;
        private bool connected;
        private int counter;

        #region champs

        private readonly string[] bandleCityChampions =
        {
            "Amumu", "Corki", "Heimerdinger", "Kennen", "Lulu", "Poppy",
            "Rumble", "Teemo", "Tristana", "Veigar", "Ziggs"
        };

        private readonly string[] bilgewaterChampions =
        {
            "Fizz", "Gangplank", "Graves", "Katarina", "Miss Fortune",
            "Nami", "Nautilus", "Twisted Fate"
        };

        private readonly string[] demaciaChampions =
        {
            "Fiora", "Galio", "Garen", "Jarvan IV", "Leona", "Lucian", "Lux",
            "Poppy", "Quinn", "Shyvana", "Sona", "Vayne", "Xin Zhao"
        };

        private readonly string[] discordChampions =
        {
            "Aatrox", "Cho'Gath", "Karthus", "Kha'Zix", "Kog'Maw", "Malzahar",
            "Nocturne", "Shaco", "Thresh", "Vel'koz"
        };

        private readonly string[] freljordChampions =
        {
            "Anivia", "Ashe", "Braum", "Lissandra", "Nunu", "Sejuani",
            "Tryndamere", "Volibear", "Gragas"
        };

        private readonly string[] ioniaChampions =
        {
            "Akali", "Irelia", "Karma", "Kennen", "Lee Sin", "Master Yi", "Shen",
            "Soraka", "Varus", "Ahri", "Yasuo"
        };

        private readonly string[] noxusChampions =
        {
            "Annie", "Cassiopeia", "Darius", "Dr. Mundo", "Draven", "Katarina",
            "LeBlanc", "Morgana", "Singed", "Sion", "Sivir", "Swain", "Talon", "Urgot", "Vladimir", "Warwick"
        };

        private readonly string[] piltoverChampions =
        {
            "Blitzcrank", "Caitlyn", "Corki", "Ezreal", "Heimerdinger",
            "Janna", "Jayce", "Jinx", "Orianna", "Vi", "Zac", "Ziggs", "Zilean"
        };

        private readonly string[] shadowIslesChampions =
        {
            "Elise", "Evelynn", "Fiddlesticks", "Hecarim", "Karthus",
            "Mordekaiser", "Nocturne", "Thresh", "Yorick", "Urgot"
        };

        private readonly string[] shurimaChampions =
        {
            "Amumu", "Malzahar", "Nasus", "Renekton", "Sivir", "Skarner",
            "Xerath", "Zilean", "Azir"
        };

        private readonly string[] voidChampions = { "Cho'Gath", "Kha'Zix", "Kog'Maw", "Malzahar", "Vel'koz" };

        private readonly string[] zaunChampions =
        {
            "Blitzcrank", "Dr. Mundo", "Janna", "Jinx", "Renekton", "Singed",
            "Twisted Fate", "Twitch", "Urgot", "Viktor", "Warwick", "Zac"
        };

        #endregion champs

        public ChampSelectPage(Page previousPage)
        {
            InitializeComponent();
            Client.OverlayContainer.Content = null;
            this.previousPage = previousPage;
            StartChampSelect();
            string sound = AmbientChampSelect.CurrentQueueToSoundFile(Client.QueueId);
            AmbientChampSelect.PlayAmbientChampSelectSound(sound);
            Client.LastPageContent = Content;
            Client.runonce = true;

            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Champion Select";
        }

        private bool BanningPhase
        {
            get { return _BanningPhase; }
            set
            {
                if (_BanningPhase != value)
                {
                    RenderChamps(value);
                    _BanningPhase = value;
                }
            }
        }

        private int LastPickTurn
        {
            get { return _LastPickTurn; }
            set
            {
                if (_LastPickTurn == value)
                    return;

                counter = configType.MainPickTimerDuration - 3;
                _LastPickTurn = value;
            }
        }

        private double MyChampId
        {
            get { return _MyChampId; }
            set
            {
                if (_MyChampId == value)
                    return;

                PlayerTradeControl.Visibility = Visibility.Hidden;
                _MyChampId = value;
            }
        }

        /// <summary>
        ///     Initializes all data required for champion select. Also retrieves latest GameDTO
        /// </summary>
        /// <summary>
        ///     Fix Champ Select
        /// </summary>
        internal static void FixChampSelect()
        {
            /*
            if (OnFixChampSelect != null)
            {
                foreach (Delegate d in OnFixChampSelect.GetInvocationList())
                {
                    PVPNet.OnMessageReceived -= (PVPNetConnection.OnMessageReceivedHandler)d;
                    OnFixChampSelect -= (PVPNetConnection.OnMessageReceivedHandler)d;
                }
            }//*/
        }

        private async void StartChampSelect()
        {
            //Force client to popup once in champion select
            Client.FocusClient();
            //Get champions and sort alphabetically

            ChampList = new List<ChampionDTO>(Client.PlayerChampions);
            ChampList.Sort(
                (x, y) =>
                    String.Compare(champions.GetChampion(x.ChampionId)
                        .displayName, champions.GetChampion(y.ChampionId).displayName, StringComparison.Ordinal));

            //Retrieve masteries and runes
            MyMasteries = Client.LoginPacket.AllSummonerData.MasteryBook;
            MyRunes = Client.LoginPacket.AllSummonerData.SpellBook;

            //Put masteries & runes into combo boxes
            int i = 0;
            foreach (MasteryBookPageDTO masteryPage in MyMasteries.BookPages)
            {
                string masteryPageName = masteryPage.Name;
                //Stop garbage mastery names
                if (masteryPageName.StartsWith("@@"))
                    masteryPageName = "Mastery Page " + ++i;

                MasteryComboBox.Items.Add(masteryPageName);
                if (masteryPage.Current)
                    MasteryComboBox.SelectedValue = masteryPageName;
            }
            i = 0;
            foreach (SpellBookPageDTO runePage in MyRunes.BookPages)
            {
                string runePageName = runePage.Name;
                //Stop garbage rune names
                if (runePageName.StartsWith("@@"))
                    runePageName = "Rune Page " + ++i;

                RuneComboBox.Items.Add(runePageName);
                if (runePage.Current)
                    RuneComboBox.SelectedValue = runePageName;
            }
            //Allow runes & masteries to be changed
            QuickLoad = true;

            //Signal to the server we are in champion select
            await Client.PVPNet.SetClientReceivedGameMessage(Client.GameID, "CHAMP_SELECT_CLIENT");
            //Selects Champion
            //New method makes it easier to instapick a champ + works in draft
            /*
            if (previousPage is TeamQueuePage && (previousPage as TeamQueuePage).SelectChampBox.Text != "Auto Select Champ")
            {
                await Client.PVPNet.SelectChampion(ChampList.FirstOrDefault(x => champions.GetChampion(x.ChampionId).displayName.ToLower() == (previousPage as TeamQueuePage).SelectChampBox.Text.ToLower()).ChampionId);
            }
            //*/
            //Retrieve the latest GameDTO
            GameDTO latestDto =
                await
                    Client.PVPNet.GetLatestGameTimerState(Client.GameID, Client.ChampSelectDTO.GameState,
                        Client.ChampSelectDTO.PickTurn);
            Joinchat(latestDto);
            //Find the game config for timers
            configType = Client.LoginPacket.GameTypeConfigs.Find(x => x.Id == latestDto.GameTypeConfigId);
            if (configType == null) //Invalid config... abort!
            {
                QuitCurrentGame();

                var overlay = new MessageOverlay
                {
                    MessageTextBox =
                    {
                        Text = "Invalid Config ID (" + latestDto.GameTypeConfigId +
                               "). Report to Eddy5641 [https://github.com/Eddy5641/LegendaryClient/issues/new]"
                    },
                    MessageTitle = { Content = "Invalid Config" }
                };
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
            }
            else
            {
                counter = configType.MainPickTimerDuration - 5;
                //Seems to be a 5 second inconsistancy with riot and what they actually provide
                CountdownTimer = new Timer();
                CountdownTimer.Tick += CountdownTimer_Tick;
                CountdownTimer.Interval = 1000; // 1 second
                CountdownTimer.Start();

                LatestDto = latestDto;
                //Get the champions for the other team to ban & sort alpabetically
                ChampionBanInfoDTO[] champsForBan = await Client.PVPNet.GetChampionsForBan();
                ChampionsForBan = new List<ChampionBanInfoDTO>(champsForBan);
                ChampionsForBan.Sort(
                    (x, y) =>
                        String.Compare(champions.GetChampion(x.ChampionId)
                            .displayName, champions.GetChampion(y.ChampionId).displayName, StringComparison.Ordinal));


                //Render our champions
                RenderChamps(false);

                //Start recieving champ select
                ChampSelect_OnMessageReceived(this, latestDto);
                Client.OnFixChampSelect += ChampSelect_OnMessageReceived;
                Client.PVPNet.OnMessageReceived += ChampSelect_OnMessageReceived;
            }
        }

        private void Joinchat(GameDTO latestDto)
        {
            if (connected)
                return;

            //Join champion select chatroom
            string jid = Client.GetChatroomJID(latestDto.RoomName.Replace("@sec", ""), latestDto.RoomPassword, false);
            Chatroom = Client.ConfManager.GetRoom(new JID(jid));
            Chatroom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            Chatroom.OnRoomMessage += Chatroom_OnRoomMessage;
            Chatroom.OnParticipantJoin += Chatroom_OnParticipantJoin;
            Chatroom.Join(latestDto.RoomPassword);
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            counter--;
            if (counter <= 0)
                return;

            LobbyTimeLabel.Content = counter;
        }

        /// <summary>
        ///     Main logic behind Champion Select
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void ChampSelect_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                #region In Champion Select

                var champDto = message as GameDTO;
                //Sometimes chat doesn't work so spam this until it does
                Joinchat(champDto);
                LatestDto = champDto;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    //Allow all champions to be selected (reset our modifications)
                    var championArray = new ListViewItem[ChampionSelectListView.Items.Count];
                    ChampionSelectListView.Items.CopyTo(championArray, 0);
                    foreach (ListViewItem y in championArray)
                    {
                        y.IsHitTestVisible = true;
                        y.Opacity = 1;
                    }

                    //Push all teams into one array to save a foreach call (looks messy)
                    if (champDto == null)
                        return;

                    var allParticipants = new List<Participant>(champDto.TeamOne.ToArray());
                    allParticipants.AddRange(champDto.TeamTwo);

                    int t = 1;

                    foreach (PlayerParticipant participant in allParticipants.Select(p => p as PlayerParticipant))
                    {
                        if (participant != null)
                        {
                            PlayerParticipant play = participant;
                            //If it is our turn to pick
                            if (play.PickTurn == champDto.PickTurn)
                            {
                                if (play.SummonerId == Client.LoginPacket.AllSummonerData.Summoner.SumId)
                                {
                                    //Allows us to instapick any champ we own. 
                                    if (Client.usingInstaPick)
                                    {
                                        bool champbanned = false;
                                        //disallow picking banned champs
                                        try
                                        {
                                            foreach (
                                                BannedChampion x in
                                                    champDto.BannedChampions.Where(
                                                        x => x.ChampionId == Client.SelectChamp))
                                                champbanned = true;

                                            //disallow picking picked champs
                                            foreach (
                                                PlayerChampionSelectionDTO selection in
                                                    champDto.PlayerChampionSelections.Where(
                                                        selection => selection.ChampionId == Client.SelectChamp))
                                                champbanned = true;

                                            var temp = new ListViewItem
                                            {
                                                Tag = Client.SelectChamp
                                            };
                                            if (!champbanned)
                                                ListViewItem_PreviewMouseDown(temp, null);

                                            Client.usingInstaPick = false;
                                        }
                                        catch
                                        {
                                            Client.Log("Something went weird insta-picking a champ", "Error");
                                        }
                                    }

                                    ChampionSelectListView.IsHitTestVisible = true;

                                    ChampionSelectListView.Opacity = 1;
                                    GameStatusLabel.Content = "Your turn to pick!";
                                    break;
                                }
                            }
                            if (String.IsNullOrEmpty(participant.SummonerName))
                            {
                                participant.SummonerName = "Summoner " + t;
                                t++;
                            }
                        }
                        //Otherwise block selection of champions unless in dev mode
                        if (!Client.Dev)
                        {
                            ChampionSelectListView.IsHitTestVisible = false;
                            ChampionSelectListView.Opacity = 0.5;
                        }
                        GameStatusLabel.Content = "Waiting for others to pick...";
                    }

                    allParticipants = allParticipants.Distinct().ToList();

                    //Champion select was cancelled 
                    if (champDto.GameState == "TEAM_SELECT")
                    {
                        if (CountdownTimer != null)
                            CountdownTimer.Stop();

                        FixChampSelect();
                        var fakePage = new FakePage
                        {
                            Content = LobbyContent
                        };
                        Client.SwitchPage(fakePage);

                        return;
                    }
                    if (champDto.GameState == "PRE_CHAMP_SELECT")
                    {
                        //Banning phase. Enable banning phase and this will render only champions for ban
                        BanningPhase = true;
                        PurpleBansLabel.Visibility = Visibility.Visible;
                        BlueBansLabel.Visibility = Visibility.Visible;
                        BlueBanListView.Visibility = Visibility.Visible;
                        PurpleBanListView.Visibility = Visibility.Visible;
                        GameStatusLabel.Content = "Bans are on-going";
                        counter = configType.BanTimerDuration - 3;

                        #region Render Bans

                        BlueBanListView.Items.Clear();
                        PurpleBanListView.Items.Clear();
                        foreach (BannedChampion x in champDto.BannedChampions)
                        {
                            var champImage = new Image
                            {
                                Height = 58,
                                Width = 58,
                                Source = champions.GetChampion(x.ChampionId).icon
                            };
                            if (x.TeamId == 100)
                                BlueBanListView.Items.Add(champImage);
                            else
                                PurpleBanListView.Items.Add(champImage);

                            foreach (ListViewItem y in championArray.Where(y => (int)y.Tag == x.ChampionId))
                            {
                                ChampionSelectListView.Items.Remove(y);
                                //Remove from arrays
                                foreach (
                                    ChampionDTO playerChamps in
                                        ChampList.ToArray()
                                            .Where(playerChamps => x.ChampionId == playerChamps.ChampionId))
                                {
                                    ChampList.Remove(playerChamps);

                                    break;
                                }

                                foreach (
                                    ChampionBanInfoDTO banChamps in
                                        ChampionsForBan.ToArray()
                                            .Where(banChamps => x.ChampionId == banChamps.ChampionId))
                                {
                                    ChampionsForBan.Remove(banChamps);

                                    break;
                                }
                            }
                        }

                        #endregion Render Bans
                    }
                    else if (champDto.GameState == "CHAMP_SELECT")
                    {
                        //Picking has started. If pickturn has changed reset timer
                        LastPickTurn = champDto.PickTurn;
                        BanningPhase = false;
                    }
                    else if (champDto.GameState == "POST_CHAMP_SELECT")
                    {
                        //Post game has started. Allow trading
                        CanTradeWith = await Client.PVPNet.GetPotentialTraders();
                        HasLockedIn = true;
                        GameStatusLabel.Content = "All players have picked!";
                        if (configType != null)
                            counter = configType.PostPickTimerDuration - 2;
                        else
                            counter = 10;
                    }
                    else if (champDto.GameState == "START_REQUESTED")
                    {
                        GameStatusLabel.Content = "The game is about to start!";
                        DodgeButton.IsEnabled = false; //Cannot dodge past this point!
                        counter = 1;
                    }
                    else if (champDto.GameState == "TERMINATED")
                    {
                        var pop = new NotifyPlayerPopup("Player Dodged", "Player has Dodged Queue.")
                        {
                            HorizontalAlignment = HorizontalAlignment.Right,
                            VerticalAlignment = VerticalAlignment.Bottom
                        };
                        Client.HasPopped = false;
                        Client.NotificationGrid.Children.Add(pop);
                        Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
                        Client.OnFixChampSelect -= ChampSelect_OnMessageReceived;
                        Client.GameStatus = "inQueue";
                        Client.SetChatHover();
                        Client.SwitchPage(previousPage);
                        Client.ClearPage(typeof(ChampSelectPage));
                        Client.ReturnButton.Visibility = Visibility.Hidden;
                    }

                    #region Display players

                    BlueListView.Items.Clear();
                    PurpleListView.Items.Clear();
                    BlueListView.Items.Refresh();
                    PurpleListView.Items.Refresh();
                    int i = 0;
                    bool purpleSide = false;

                    //Aram hack, view other players champions & names (thanks to Andrew)
                    var otherPlayers = new List<PlayerChampionSelectionDTO>(champDto.PlayerChampionSelections.ToArray());
                    AreWePurpleSide = false;

                    foreach (Participant participant in allParticipants)
                    {
                        Participant tempParticipant = participant;
                        i++;
                        var control = new ChampSelectPlayer();
                        //Cast AramPlayers as PlayerParticipants. This removes reroll data
                        if (tempParticipant is AramPlayerParticipant)
                            tempParticipant = new PlayerParticipant(tempParticipant.GetBaseTypedObject());

                        if (tempParticipant is PlayerParticipant)
                        {
                            var player = tempParticipant as PlayerParticipant;
                            if (!String.IsNullOrEmpty(player.SummonerName))
                                control.PlayerName.Content = player.SummonerName;
                            else
                            {
                                AllPublicSummonerDataDTO summoner =
                                    await Client.PVPNet.GetAllPublicSummonerDataByAccount(player.SummonerId);
                                if (summoner.Summoner != null && !String.IsNullOrEmpty(summoner.Summoner.Name))
                                    control.PlayerName.Content = summoner.Summoner.Name;
                                else
                                    control.PlayerName.Content = "Unknown Player";
                            }

                            foreach (PlayerChampionSelectionDTO selection in champDto.PlayerChampionSelections)
                            {
                                #region Disable picking selected champs

                                PlayerChampionSelectionDTO selection1 = selection;
                                foreach (
                                    ListViewItem y in championArray.Where(y => (int)y.Tag == selection1.ChampionId))
                                {
                                    y.IsHitTestVisible = true;
                                    y.Opacity = 0.5;
                                    if (configType == null)
                                        continue;

                                    if (!configType.DuplicatePick)
                                        continue;

                                    y.IsHitTestVisible = false;
                                    y.Opacity = 1;
                                }

                                #endregion Disable picking selected champs

                                if (selection.SummonerInternalName != player.SummonerInternalName)
                                    continue;

                                //Clear our teams champion selection for aram hack
                                otherPlayers.Remove(selection);
                                control = RenderPlayer(selection, player);
                                //If we have locked in render skin select
                                if (!HasLockedIn ||
                                    selection.SummonerInternalName !=
                                    Client.LoginPacket.AllSummonerData.Summoner.InternalName || Client.Dev)
                                    continue;

                                if (purpleSide)
                                    AreWePurpleSide = true;

                                RenderLockInGrid(selection);
                                if (player.PointSummary == null)
                                    continue;

                                LockInButton.Content = string.Format("Reroll ({0}/{1})",
                                    player.PointSummary.CurrentPoints, player.PointSummary.PointsCostToRoll);
                                LockInButton.IsEnabled = player.PointSummary.NumberOfRolls > 0;
                            }
                        }
                        else if (tempParticipant is ObfuscatedParticipant)
                        {
                            control.PlayerName.Content = "Summoner " +
                                                         ((tempParticipant as ObfuscatedParticipant).GameUniqueId -
                                                          ((tempParticipant as ObfuscatedParticipant).GameUniqueId > 5
                                                              ? 5
                                                              : 0));
                        }
                        else if (tempParticipant is BotParticipant)
                        {
                            var bot = tempParticipant as BotParticipant;
                            string botChamp = bot.SummonerInternalName.Split('_')[1]; //Why is this internal name rito?
                            champions botSelectedChamp = champions.GetChampion(botChamp);
                            var part = new PlayerParticipant();
                            var selection = new PlayerChampionSelectionDTO
                            {
                                ChampionId = botSelectedChamp.id
                            };
                            part.SummonerName = botSelectedChamp.displayName + " bot";
                            control = RenderPlayer(selection, part);
                        }
                        else
                            control.PlayerName.Content = "Unknown Summoner";

                        //Display purple side if we have gone through our team
                        if (i > champDto.TeamOne.Count)
                        {
                            i = 0;
                            purpleSide = true;
                        }

                        if (!purpleSide)
                            BlueListView.Items.Add(control);
                        else
                            PurpleListView.Items.Add(control);
                    }

                    //Do aram hack!
                    if (otherPlayers.Count <= 0)
                        return;

                    if (AreWePurpleSide)
                        BlueListView.Items.Clear();
                    else
                        PurpleListView.Items.Clear();

                    foreach (PlayerChampionSelectionDTO hackSelection in otherPlayers)
                    {
                        var player = new PlayerParticipant
                        {
                            SummonerName = hackSelection.SummonerInternalName
                        };
                        ChampSelectPlayer control = RenderPlayer(hackSelection, player);
                        if (AreWePurpleSide)
                            BlueListView.Items.Add(control);
                        else
                            PurpleListView.Items.Add(control);
                    }

                    #endregion Display players
                }));

                #endregion In Champion Select
            }
            else if (message.GetType() == typeof(PlayerCredentialsDto))
            {
                Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;

                #region Launching Game

                var dto = message as PlayerCredentialsDto;
                Client.CurrentGame = dto;

                if (HasLaunchedGame)
                    return;

                HasLaunchedGame = true;
                if (Settings.Default.AutoRecordGames)
                {
                    Dispatcher.InvokeAsync(async () =>
                    {
                        PlatformGameLifecycleDTO n =
                            await
                                Client.PVPNet.RetrieveInProgressSpectatorGameInfo(
                                    Client.LoginPacket.AllSummonerData.Summoner.Name);
                        if (n.GameName != null)
                        {
                            string ip = n.PlayerCredentials.ObserverServerIp + ":" +
                                        n.PlayerCredentials.ObserverServerPort;
                            string key = n.PlayerCredentials.ObserverEncryptionKey;
                            var gameId = (Int32)n.PlayerCredentials.GameId;
                            new ReplayRecorder(ip, gameId, Client.Region.InternalName, key);
                        }
                    });
                }
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    if (CountdownTimer != null)
                        CountdownTimer.Stop();

                    Client.LaunchGame();
                    InGame();
                    Client.ReturnButton.Visibility = Visibility.Hidden;
                    Client.AmbientSoundPlayer.Stop();
                }));

                #endregion Launching Game
            }
            else if (message.GetType() == typeof(TradeContractDTO))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var tradeDto = message as TradeContractDTO;
                    if (tradeDto == null)
                        return;

                    switch (tradeDto.State)
                    {
                        case "PENDING":
                            {
                                PlayerTradeControl.Visibility = Visibility.Visible;
                                PlayerTradeControl.Tag = tradeDto;
                                PlayerTradeControl.AcceptButton.Visibility = Visibility.Visible;
                                PlayerTradeControl.DeclineButton.Content = "Decline";

                                champions myChampion = champions.GetChampion((int)tradeDto.ResponderChampionId);
                                PlayerTradeControl.MyChampImage.Source = myChampion.icon;
                                PlayerTradeControl.MyChampLabel.Content = myChampion.displayName;
                                champions theirChampion = champions.GetChampion((int)tradeDto.RequesterChampionId);
                                PlayerTradeControl.TheirChampImage.Source = theirChampion.icon;
                                PlayerTradeControl.TheirChampLabel.Content = theirChampion.displayName;
                                PlayerTradeControl.RequestLabel.Content = string.Format("{0} wants to trade!",
                                    tradeDto.RequesterInternalSummonerName);
                            }
                            break;
                        case "BUSY":
                        case "DECLINED":
                        case "CANCELED":
                            {
                                PlayerTradeControl.Visibility = Visibility.Hidden;

                                TextInfo text = new CultureInfo("en-US", false).TextInfo;
                                var pop = new NotificationPopup(ChatSubjects.INVITE_STATUS_CHANGED,
                                    string.Format("{0} has {1} this trade", tradeDto.RequesterInternalSummonerName,
                                        text.ToTitleCase(tradeDto.State)));

                                if (tradeDto.State == "BUSY")
                                    pop.NotificationTextBox.Text = string.Format("{0} is currently busy",
                                        tradeDto.RequesterInternalSummonerName);

                                pop.Height = 200;
                                pop.OkButton.Visibility = Visibility.Visible;
                                pop.HorizontalAlignment = HorizontalAlignment.Right;
                                pop.VerticalAlignment = VerticalAlignment.Bottom;
                                Client.NotificationGrid.Children.Add(pop); //*/
                            }
                            break;
                    }
                }));
            }
        }


        /// <summary>
        ///     Render the post selection grid after locked in
        /// </summary>
        /// <param name="selection">Details of champion you want to render</param>
        internal void RenderLockInGrid(PlayerChampionSelectionDTO selection)
        {
            ChampionSelectListView.Visibility = Visibility.Hidden;
            AfterChampionSelectGrid.Visibility = Visibility.Visible;

            LockInButton.Content = "Locked In";

            Switch.IsChecked = true;

            ChangeSelectedChampionSkins(selection.ChampionId);
        }

        internal void ChangeSelectedChampionSkins(int selectedChampionId)
        {
            champions champion = champions.GetChampion(selectedChampionId);
            if (champion == null)
                return;

            SkinSelectListView.Items.Clear();
            AbilityListView.Items.Clear();

            //Render default skin
            var item = new ListViewItem();
            var skinImage = new Image();
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champion.portraitPath)))
            {
                string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champion.portraitPath);
                skinImage.Source = Client.GetImage(uriSource);
                skinImage.Width = 191;
                skinImage.Stretch = Stretch.UniformToFill;
                item.Tag = "0:" + champion.id; //Hack
                item.Content = skinImage;
                SkinSelectListView.Items.Add(item);
                //Render abilities
                List<championAbilities> Abilities = championAbilities.GetAbilities(selectedChampionId);
                var abilities = new List<ChampionAbility>();
                foreach (championAbilities ability in Abilities)
                {
                    var championAbility = new ChampionAbility();
                    uriSource = Path.Combine(Client.ExecutingDirectory, "Assets",
                        ability.iconPath.ToLower().Contains("passive") ? "passive" : "spell", ability.iconPath);
                    championAbility.AbilityImage.Source = Client.GetImage(uriSource);
                    if (!String.IsNullOrEmpty(ability.hotkey))
                        championAbility.AbilityHotKey.Content = ability.hotkey;

                    switch (ability.hotkey)
                    {
                        case "":
                            championAbility.Order = 0;
                            break;
                        case "Q":
                            championAbility.Order = 1;
                            break;
                        case "W":
                            championAbility.Order = 2;
                            break;
                        case "E":
                            championAbility.Order = 3;
                            break;
                        case "R":
                            championAbility.Order = 4;
                            break;
                    }
                    championAbility.AbilityName.Content = ability.name;
                    championAbility.AbilityDescription.Text = ability.description;
                    championAbility.Width = 375;
                    championAbility.Height = 75;
                    abilities.Add(championAbility);
                }
                abilities.Sort();
                foreach (ChampionAbility a in abilities)
                    AbilityListView.Items.Add(a);

                //Render champions
                foreach (ChampionSkinDTO skin in from champ in ChampList
                                                 where champ.ChampionId == selectedChampionId
                                                 from skin in champ.ChampionSkins
                                                 where skin.Owned
                                                 select skin)
                {
                    item = new ListViewItem();
                    skinImage = new Image();
                    uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                        championSkins.GetSkin(skin.SkinId).portraitPath);
                    skinImage.Source = Client.GetImage(uriSource);
                    skinImage.Width = 191;
                    skinImage.Stretch = Stretch.UniformToFill;
                    item.Tag = skin.SkinId;
                    item.Content = skinImage;
                    SkinSelectListView.Items.Add(item);
                }
            }
            else
                Client.Log("File not found.");
        }

        /// <summary>
        ///     Render all champions
        /// </summary>
        /// <param name="renderBans">Render champions for ban</param>
        internal void RenderChamps(bool renderBans)
        {
            ChampionSelectListView.Items.Clear();
            if (!renderBans)
            {
                foreach (ChampionDTO champ in ChampList)
                {
                    champions getChamp = champions.GetChampion(champ.ChampionId);
                    if (previousPage.GetType() == typeof(FactionsGameLobbyPage))
                    {
                        var page = previousPage as FactionsGameLobbyPage;
                        if (page != null)
                        {
                            LeftTeamLabel.Content = page.GetLeftTeam();
                            RightTeamLabel.Content = page.GetRightTeam();
                            string myTeam = (AreWePurpleSide) ? page.GetRightTeam() : page.GetLeftTeam();
                            switch (myTeam)
                            {
                                case "Ionia":
                                    if (!ioniaChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Bilgewater":
                                    if (!bilgewaterChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Freljord":
                                    if (!freljordChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Shadow Isles":
                                    if (!shadowIslesChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Demacia":
                                    if (!demaciaChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Noxus":
                                    if (!noxusChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Piltover":
                                    if (!piltoverChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Bandle City":
                                    if (!bandleCityChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Zaun":
                                    if (!zaunChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Void":
                                    if (!voidChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Shurima":
                                    if (!shurimaChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Discord":
                                    if (!discordChampions.Contains(getChamp.displayName)) continue;
                                    break;
                            }
                        }
                    }
                    if ((!champ.Owned && !champ.FreeToPlay) ||
                        !getChamp.displayName.ToLower().Contains(SearchTextBox.Text.ToLower()))
                        continue;

                    //Add to ListView
                    var item = new ListViewItem();
                    var championImage = new ChampionImage
                    {
                        ChampImage = { Source = champions.GetChampion(champ.ChampionId).icon }
                    };
                    if (champ.FreeToPlay)
                        championImage.FreeToPlayLabel.Visibility = Visibility.Visible;

                    championImage.Width = 64;
                    championImage.Height = 64;
                    item.Tag = champ.ChampionId;
                    item.Content = championImage.Content;
                    ChampionSelectListView.Items.Add(item);
                }
            }
            else
            {
                foreach (ChampionBanInfoDTO champ in ChampionsForBan)
                {
                    champions getChamp = champions.GetChampion(champ.ChampionId);
                    if (!champ.EnemyOwned || !getChamp.displayName.ToLower().Contains(SearchTextBox.Text.ToLower()))
                        continue;

                    //Add to ListView
                    var item = new ListViewItem();
                    var championImage = new ChampionImage
                    {
                        ChampImage = { Source = champions.GetChampion(champ.ChampionId).icon },
                        Width = 64,
                        Height = 64
                    };
                    item.Tag = champ.ChampionId;
                    item.Content = championImage.Content;
                    ChampionSelectListView.Items.Add(item);
                }
            }
        }

        /// <summary>
        ///     Render individual players
        /// </summary>
        /// <param name="selection">The champion the player has selected</param>
        /// <param name="player">The participant details of the player</param>
        /// <returns></returns>
        internal ChampSelectPlayer RenderPlayer(PlayerChampionSelectionDTO selection, PlayerParticipant player)
        {
            var control = new ChampSelectPlayer();
            //Render champion
            if (selection.ChampionId != 0)
                control.ChampionImage.Source = champions.GetChampion(selection.ChampionId).icon;

            //Render summoner spells
            if (selection.Spell1Id != 0)
            {
                string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                    SummonerSpell.GetSpellImageName((int)selection.Spell1Id));
                control.SummonerSpell1.Source = Client.GetImage(uriSource);
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                    SummonerSpell.GetSpellImageName((int)selection.Spell2Id));
                control.SummonerSpell2.Source = Client.GetImage(uriSource);
            }
            //Set our summoner spells in client
            if (player.SummonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
            {
                string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                    SummonerSpell.GetSpellImageName((int)selection.Spell1Id));
                SummonerSpell1Image.Source = Client.GetImage(uriSource);
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                    SummonerSpell.GetSpellImageName((int)selection.Spell2Id));
                SummonerSpell2Image.Source = Client.GetImage(uriSource);
                MyChampId = selection.ChampionId;
            }
            //Has locked in
            if (player.PickMode == 2)
            {
                const string uriSource = "/LegendaryClient;component/Locked.png";
                control.LockedInIcon.Source = Client.GetImage(uriSource);
            }
            //Make obvious whos pick turn it is
            if (player.PickTurn != LatestDto.PickTurn &&
                (LatestDto.GameState == "CHAMP_SELECT" || LatestDto.GameState == "PRE_CHAMP_SELECT"))
                control.Opacity = 0.5;
            else
                //Full opacity when not picking or banning
                control.Opacity = 1;

            //If trading with this player is possible
            if (CanTradeWith != null && (CanTradeWith.PotentialTraders.Contains(player.SummonerInternalName) || Client.Dev))
                control.TradeButton.Visibility = Visibility.Visible;

            //If this player is duo/trio/quadra queued with players
            if (player.TeamParticipantId != null && (double)player.TeamParticipantId != 0)
            {
                //Byte hack to get individual hex colors
                byte[] values = BitConverter.GetBytes((double)player.TeamParticipantId);
                if (!BitConverter.IsLittleEndian) Array.Reverse(values);

                byte r = values[2];
                byte b = values[3];
                byte g = values[4];

                Color myColor = Color.FromArgb(r, b, g);

                var converter = new BrushConverter();
                var brush = (Brush)converter.ConvertFromString("#" + myColor.Name);
                control.TeamRectangle.Fill = brush;
                control.TeamRectangle.Visibility = Visibility.Visible;
            }
            control.LockedInIcon.Visibility = Visibility.Visible;
            control.TradeButton.Tag = new KeyValuePair<PlayerChampionSelectionDTO, PlayerParticipant>(selection, player);
            control.TradeButton.Click += TradeButton_Click;
            control.PlayerName.Content = player.SummonerName;

            return control;
        }

        private async void TradeButton_Click(object sender, RoutedEventArgs e)
        {
            var p = (KeyValuePair<PlayerChampionSelectionDTO, PlayerParticipant>)((Button)sender).Tag;
            await Client.PVPNet.AttemptTrade(p.Value.SummonerInternalName, p.Key.ChampionId);

            PlayerTradeControl.Visibility = Visibility.Visible;
            champions myChampion = champions.GetChampion((int)MyChampId);
            PlayerTradeControl.MyChampImage.Source = myChampion.icon;
            PlayerTradeControl.MyChampLabel.Content = myChampion.displayName;
            champions theirChampion = champions.GetChampion(p.Key.ChampionId);
            PlayerTradeControl.TheirChampImage.Source = theirChampion.icon;
            PlayerTradeControl.TheirChampLabel.Content = theirChampion.displayName;
            PlayerTradeControl.RequestLabel.Content = "Sent trade request...";
            PlayerTradeControl.AcceptButton.Visibility = Visibility.Hidden;
            PlayerTradeControl.DeclineButton.Content = "Cancel";
        }

        private async void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item == null)
                return;

            if (!BanningPhase)
            {
                if (item.Tag == null)
                    return;
                //SelectChampion.SelectChampion(selection.ChampionId)
                await Client.PVPNet.SelectChampion(SelectChampion.SelectChamp((int)item.Tag));

                //TODO: Fix stupid animation glitch on left hand side
                var fadingAnimation = new DoubleAnimation
                {
                    From = 0.4,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };
                fadingAnimation.Completed += (eSender, eArgs) =>
                {
                    string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                        champions.GetChampion((int)item.Tag).splashPath);
                    BackgroundSplash.Source = Client.GetImage(uriSource);
                    fadingAnimation = new DoubleAnimation
                    {
                        From = 0,
                        To = 0.4,
                        Duration = new Duration(TimeSpan.FromSeconds(0.5))
                    };

                    BackgroundSplash.BeginAnimation(OpacityProperty, fadingAnimation);
                };

                BackgroundSplash.BeginAnimation(OpacityProperty, fadingAnimation);
                ChangeSelectedChampionSkins((int)item.Tag);
            }
            else if (item.Tag != null)
                await Client.PVPNet.BanChampion((int)item.Tag);
        }

        private async void SkinSelectListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item == null)
                return;

            if (item.Tag == null)
                return;

            var s = item.Tag as string;
            if (s != null)
            {
                string[] splitItem = s.Split(':');
                int championId = Convert.ToInt32(splitItem[1]);
                champions champion = champions.GetChampion(championId);
                await Client.PVPNet.SelectChampionSkin(championId, 0);
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = "Selected Default " + champion.name + " as skin" + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }
            else
            {
                championSkins skin = championSkins.GetSkin((int)item.Tag);
                await Client.PVPNet.SelectChampionSkin(skin.championId, skin.id);
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = "Selected " + skin.displayName + " as skin" + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }
            ChatText.ScrollToEnd();
        }

        private void DodgeButton_Click(object sender, RoutedEventArgs e)
        {
            Warning Warn = new Warning();
            Warn.Title.Content = "Dodge Game";
            Warn.MessageText.Text = "Are You Sure You Want To Dodge? You will still have to spend the time penalty";
            Warn.backtochampselect.Click += (o, m) =>
            {
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
            };
            Warn.AcceptButton.Click += (o, m) =>
            {
                QuitCurrentGame();
            };
            Warn.hide.Click += (o, m) =>
            {
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
            };
            Warn.backtochampselect.Content = "Return to Champ Select";
            Warn.AcceptButton.Content = "Dodge game";
            Client.FullNotificationOverlayContainer.Content = Warn.Content;
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;

        }

        private async void QuitCurrentGame()
        {
            Client.AmbientSoundPlayer.Stop();
            await Client.PVPNet.QuitGame();
            Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
            Client.ClearPage(typeof(CustomGameLobbyPage));
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.ClearPage(typeof(ChampSelectPage));
            Client.GameStatus = "outOfGame";
            Client.SetChatHover();
            uiLogic.UpdateMainPage();
        }

        private async void InGame()
        {
            await Client.PVPNet.QuitGame();
            Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
            Client.ClearPage(typeof(CustomGameLobbyPage));
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.ClearPage(typeof(ChampSelectPage));
            Client.ClearPage(typeof(FactionsCreateGamePage));
            Client.GameStatus = "inGame";
            Client.timeStampSince =
                (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalMilliseconds;
            Client.SetChatHover();

            Client.SwitchPage(new InGame());
        }

        private async void LockInButton_Click(object sender, RoutedEventArgs e)
        {
            if (configType.PickMode != "AllRandomPickStrategy")
            {
                if (ChampionSelectListView.SelectedItems.Count <= 0)
                    return;

                await Client.PVPNet.ChampionSelectCompleted();
                HasLockedIn = true;
            }
            else
            {
                await Client.PVPNet.Roll();
                HasLockedIn = true;
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RenderChamps(BanningPhase);
        }

        private void EditMasteriesButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new MasteriesOverlay().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void EditRunesButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new RunesOverlay().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void SummonerSpell_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new SelectSummonerSpells(LatestDto.GameMode).Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private async void MasteryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!QuickLoad) //Make loading quicker
                return;

            bool hasChanged = false;
            int i = 0;
            var bookDto = new MasteryBookDTO
            {
                SummonerId = Client.LoginPacket.AllSummonerData.Summoner.SumId,
                BookPages = new List<MasteryBookPageDTO>()
            };
            foreach (MasteryBookPageDTO masteryPage in MyMasteries.BookPages)
            {
                string masteryPageName = masteryPage.Name;
                //Convert garbage to readable so we get the proper mastery page
                if (masteryPageName.StartsWith("@@"))
                    masteryPageName = "Mastery Page " + ++i;

                masteryPage.Current = false;
                if (masteryPageName == (string)MasteryComboBox.SelectedItem)
                {
                    masteryPage.Current = true;
                    hasChanged = true;
                    var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                    {
                        Text = "Selected " + masteryPageName + " as Mastery Page" + Environment.NewLine
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    ChatText.ScrollToEnd();
                }
                bookDto.BookPages.Add(masteryPage);
            }
            if (hasChanged)
                await Client.PVPNet.SaveMasteryBook(bookDto);
        }

        private async void RuneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!QuickLoad) //Make loading quicker
                return;

            var selectedRunePage = new SpellBookPageDTO();
            int i = 0;
            bool hasChanged = false;
            foreach (SpellBookPageDTO runePage in MyRunes.BookPages)
            {
                string runePageName = runePage.Name;
                if (runePageName.StartsWith("@@"))
                    runePageName = "Rune Page " + ++i;

                runePage.Current = false;
                if (runePageName != (string)RuneComboBox.SelectedItem)
                    continue;

                runePage.Current = true;
                selectedRunePage = runePage;
                hasChanged = true;
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = "Selected " + runePageName + " as Rune Page" + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                ChatText.ScrollToEnd();
            }
            if (hasChanged)
                await Client.PVPNet.SelectDefaultSpellBookPage(selectedRunePage);
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            //Enable dev mode if !~dev is typed in chat
            if (ChatTextBox.Text == "!~dev")
            {
                if (!Client.Dev)
                {
                    var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                    {
                        Text = "You are not a dev." + Environment.NewLine
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                }
                ChatTextBox.Text = "";
            }
            else
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": "
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (Client.Filter)
                    tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
                else
                    tr.Text = ChatTextBox.Text + Environment.NewLine;

                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                Chatroom.PublicMessage(ChatTextBox.Text);
                ChatTextBox.Text = "";
                ChatText.ScrollToEnd();
            }
        }

        private void Chatroom_OnRoomMessage(object sender, Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                //Ignore the message that is always sent when joining
                if (msg.Body == "This room is not anonymous")
                    return;

                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.From.Resource + ": "
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (Client.Filter)
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "").Filter() +
                              Environment.NewLine;
                else
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;

                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                ChatText.ScrollToEnd();
            }));
        }

        private void Chatroom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            connected = true;
            if (Client.InstaCall)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                    {
                        Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": "
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.OrangeRed);
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    if (Client.Filter)
                        tr.Text = Client.CallString.Filter() + Environment.NewLine;
                    else
                        tr.Text = Client.CallString + Environment.NewLine;

                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    Chatroom.PublicMessage(Client.CallString);
                    ChatText.ScrollToEnd();
                    var t = new Timer
                    {
                        Interval = 10000
                    };
                    t.Start();
                    t.Tick += (o, e) =>
                    {
                        Client.InstaCall = false;
                        t.Stop();
                    };
                }));
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                //Solve multipile joins


                if (PreviousPlayers.Any(previousPlayer => previousPlayer == participant.Nick))
                {
                    Chatroom.OnRoomMessage -= Chatroom_OnRoomMessage;
                    Chatroom.OnParticipantJoin -= Chatroom_OnParticipantJoin;

                    return;
                }

                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = participant.Nick + " joined the room." + Environment.NewLine
                };

                PreviousPlayers.Add(participant.Nick);
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatText.ScrollToEnd();
            }));
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            if (!Switch.IsChecked.HasValue)
                return;

            if ((bool)Switch.IsChecked)
            {
                Switch.Content = "Champions";
                ChampionSelectListView.Visibility = Visibility.Hidden;
                AfterChampionSelectGrid.Visibility = Visibility.Visible;
            }
            else
            {
                Switch.Content = "Skins";
                ChampionSelectListView.Visibility = Visibility.Visible;
                AfterChampionSelectGrid.Visibility = Visibility.Hidden;
            }
        }
    }
}