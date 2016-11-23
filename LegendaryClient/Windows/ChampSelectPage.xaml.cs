using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.Replays;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform;
using LegendaryClient.Logic.SoundLogic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RtmpSharp.Messaging;
using Color = System.Drawing.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using ListViewItem = System.Windows.Controls.ListViewItem;
using Timer = System.Windows.Forms.Timer;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using agsXMPP;
using agsXMPP.Collections;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ChampSelectPage.xaml
    /// </summary>
    public partial class ChampSelectPage
    {
        private readonly List<string> PreviousPlayers = new List<string>(); //Needs to be initialized!
        private Page previousPage;

        private bool AreWePurpleSide;
        private PotentialTradersDTO CanTradeWith;
        private List<ChampionDTO> ChampList;
        private List<ChampionBanInfoDTO> ChampionsForBan;
        private MucManager Chatroom;
        private Timer CountdownTimer;
        private bool HasLaunchedGame;
        private bool HasLockedIn;
        private bool CanLockIn;
        private GameDTO LatestDto;
        private MasteryBookDTO MyMasteries;
        private SpellBookDTO MyRunes;
        private bool QuickLoad; //Don't load masteries and runes on load at start
        private bool _BanningPhase;
        private int _LastPickTurn;
        private double _MyChampId;
        private GameTypeConfigDTO configType;
        private int counter;
        private List<int> disabledCharacters = new List<int>();
        private string firstPlayer = null;
        private Jid jid;
        private List<int> Skins;

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

        private readonly string[] shUrimaChampions =
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

        public ChampSelectPage(string RoomName, string RoomPassword)
        {
            InitializeComponent();
            var Jid = Client.GetChatroomJid(RoomName.Replace("@sec", ""), RoomPassword, false);
            jid = new Jid(Jid);
            Chatroom = new MucManager(Client.XmppConnection);
            Client.XmppConnection.OnMessage += XmppConnection_OnMessage;
            Client.XmppConnection.OnPresence += XmppConnection_OnPresence;
            Chatroom.AcceptDefaultConfiguration(jid);
            Chatroom.JoinRoom(jid, Client.LoginPacket.AllSummonerData.Summoner.Name, RoomPassword);
        }

        void XmppConnection_OnPresence(object sender, Presence pres)
        {
            if (jid.Bare.Contains(pres.From.User))
                return;


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
                    Client.XmppConnection.Send(new Message(jid, Client.CallString));
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
                if (firstPlayer == null)
                    firstPlayer = pres.From.User;
                else
                {
                    if (firstPlayer == pres.From.User)
                    {
                        Client.XmppConnection.MessageGrabber.Remove(jid);
                        Client.XmppConnection.OnPresence -= XmppConnection_OnPresence;
                    }
                }

                if (PreviousPlayers.Any(previousPlayer => previousPlayer == pres.From.User))
                {
                    return;
                }

                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = pres.From.Resource + " joined the room." + Environment.NewLine
                };

                PreviousPlayers.Add(pres.From.User);
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatText.ScrollToEnd();
            }));
        }

        void XmppConnection_OnMessage(object sender, Message msg)
        {
            if (jid.Bare.Contains(msg.From.User))
                return;

            if (msg.From.Resource == Client.LoginPacket.AllSummonerData.Summoner.Name)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                //Ignore the message that is always sent when joining
                if (msg.Body == "This room is not anonymous")
                    return;

                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.From.Resource + ": "
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Turquoise);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (Client.Filter)
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "").Filter() +
                              Environment.NewLine;
                else
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;

                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                ChatText.ScrollToEnd();
            }));
        }

        public ChampSelectPage Load(Page previousPage)
        {
            Client.ClearPage(typeof(QueuePopOverlay));
            Client.inQueueTimer.Visibility = Visibility.Hidden;
            Client.OverlayContainer.Content = null;
            this.previousPage = previousPage;
            StartChampSelect();
            if (!Settings.Default.DisableClientSound)
            {
                var sound = AmbientChampSelect.CurrentQueueToSoundFile(Client.QueueId);
                AmbientChampSelect.PlayAmbientChampSelectSound(sound);
            }
            Client.LastPageContent = Content;
            Client.runonce = true;
            GetLocalRunePages();

            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Champion Select";
            return this;
        }

        private void GetLocalRunePages()
        {
            foreach (var item in Client.LocalRunePages)
            {
                LocalRuneComboBox.Items.Add(item.Key);
            }
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

        private async void StartChampSelect()
        {
            //Force client to popup once in champion select
            Client.FocusClient();
            //Get champions and sort alphabetically

            CanLockIn = false;
            ChampList = new List<ChampionDTO>(Client.PlayerChampions);
            ChampList.Sort(
                (x, y) =>
                    string.Compare(champions.GetChampion(x.ChampionId)
                        .displayName, champions.GetChampion(y.ChampionId).displayName, StringComparison.Ordinal));
            if (Client.LoginPacket.ClientSystemStates.freeToPlayChampionsForNewPlayersMaxLevel >=
                Client.LoginPacket.AllSummonerData.SummonerLevel.Level)
                foreach (var item in ChampList)
                {
                    if (Client.LoginPacket.ClientSystemStates.freeToPlayChampionIdList.Contains(item.ChampionId))
                        item.FreeToPlay = true;
                    else
                        item.FreeToPlay = false;
                }

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
            await RiotCalls.SetClientReceivedGameMessage(Client.GameID, "CHAMP_SELECT_CLIENT");
            GameDTO latestDto =
                await
                    RiotCalls.GetLatestGameTimerState(Client.GameID, Client.ChampSelectDTO.GameState,
                        Client.ChampSelectDTO.PickTurn);
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
                if (latestDto.GameState.ToUpper() == "PRE_CHAMP_SELECT")
                {
                    ChampionBanInfoDTO[] champsForBan = await RiotCalls.GetChampionsForBan();
                    ChampionsForBan = new List<ChampionBanInfoDTO>(champsForBan);
                    ChampionsForBan.Sort(
                        (x, y) =>
                            string.Compare(champions.GetChampion(x.ChampionId)
                                .displayName, champions.GetChampion(y.ChampionId).displayName, StringComparison.Ordinal));
                }


                //Render our champions
                RenderChamps(false);

                //Start recieving champ select
                Select_OnMessageReceived(this, latestDto);
                //Client.OnFixChampSelect += ChampSelect_OnMessageReceived;
                Client.RiotConnection.MessageReceived += ChampSelect_OnMessageReceived;
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            counter--;
            if (counter <= 0)
                return;
            //TODO: Show an arrow to which side is picking/banning for draft
            LobbyTimeLabel.Content = counter;
            LobbyTimeLabel2.Content = counter;
        }

        private void ChampSelect_OnMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            Select_OnMessageReceived(sender, message.Body);
        }

        /// <summary>
        ///     Main logic behind Champion Select
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void Select_OnMessageReceived(object sender, object message)
        {
            if (message is GameDTO)
            {
                #region In Champion Select

                var champDto = message as GameDTO;
                //Sometimes chat doesn't work so spam this until it does
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

                    if (LatestDto.QueueTypeName == "COUNTER_PICK") //fix for nemesis draft, get your champ from GameDTO
                    {
                        var selectedChamp = champDto.PlayerChampionSelections.Find(item => item.SummonerInternalName == Client.LoginPacket.AllSummonerData.Summoner.InternalName);
                        ChangeSelectedChampionSkins(selectedChamp.ChampionId);
                    }
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
                                    if (Settings.Default.PickBanFocus)
                                        Client.MainWin.Focus();
                                    if (Settings.Default.PickBanFlash)
                                        Client.MainWin.FlashWindow();
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
                            if (string.IsNullOrEmpty(participant.SummonerName))
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

                        Client.FixChampSelect();

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

                            foreach (var y in championArray.Where(y => (int)y.Tag == x.ChampionId))
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
                        CanTradeWith = await RiotCalls.GetPotentialTraders();
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
                        Client.CurrentPage = previousPage;
                        Client.HasPopped = false;
                        Client.ReturnButton.Content = "Return to Lobby Page";
                        Client.ReturnButton.Visibility = Visibility.Visible;
                        Client.inQueueTimer.Visibility = Visibility.Visible;
                        Client.NotificationGrid.Children.Add(pop);
                        Client.RiotConnection.MessageReceived -= ChampSelect_OnMessageReceived;
                        //Client.OnFixChampSelect -= ChampSelect_OnMessageReceived;
                        Client.GameStatus = "inQueue";
                        Client.SetChatHover();
                        Client.SwitchPage(previousPage);
                        Client.ClearPage(typeof(ChampSelectPage));

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
                        {
                            tempParticipant = (PlayerParticipant)tempParticipant;
                        }

                        if (tempParticipant is PlayerParticipant)
                        {
                            var player = tempParticipant as PlayerParticipant;
                            if (!string.IsNullOrEmpty(player.SummonerName))
                            {
                                control.PlayerName.Content = player.SummonerName;
                                control.sumName = player.SummonerName;
                            }
                            else
                            {
                                try
                                {
                                    AllPublicSummonerDataDTO summoner =
                                        await RiotCalls.GetAllPublicSummonerDataByAccount(player.SummonerId);
                                    if (summoner.Summoner != null && !string.IsNullOrEmpty(summoner.Summoner.Name))
                                        control.PlayerName.Content = summoner.Summoner.Name;
                                    else
                                        control.PlayerName.Content = "Unknown Player";
                                }
                                catch
                                {
                                    control.PlayerName.Content = "Unknown Player";
                                }
                            }

                            foreach (PlayerChampionSelectionDTO selection in champDto.PlayerChampionSelections)
                            {
                                #region Disable picking selected champs

                                PlayerChampionSelectionDTO selection1 = selection;
                                foreach (ListViewItem y in championArray.Where(y => (int)y.Tag == selection1.ChampionId))
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

                                foreach (ListViewItem y in championArray.Where(y => disabledCharacters.Contains((int)y.Tag)))
                                {
                                    y.Opacity = .7;
                                    y.IsHitTestVisible = false;
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
                                    Client.LoginPacket.AllSummonerData.Summoner.InternalName || (Client.Dev && champDto.MapId != 12))
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
                            if (bot.SummonerInternalName.Contains('_'))
                            {
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
                            {
                                control.PlayerName.Content = "Bot";
                                control.sumName = "Bot";
                            }
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
            else if (message is PlayerCredentialsDto)
            {
                Client.RiotConnection.MessageReceived -= ChampSelect_OnMessageReceived;

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
                                RiotCalls.RetrieveInProgressSpectatorGameInfo(
                                    Client.LoginPacket.AllSummonerData.Summoner.Name);
                        if (n.GameName != null)
                        {
                            string ip = n.PlayerCredentials.ObserverServerIp + ":" +
                                        n.PlayerCredentials.ObserverServerPort;
                            string key = n.PlayerCredentials.ObserverEncryptionKey;
                            var gameId = (int)n.PlayerCredentials.GameId;
                            new ReplayRecorder(ip, gameId, Client.Region.InternalName, key);
                        }
                    });
                }
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    if (CountdownTimer != null)
                        CountdownTimer.Stop();

                    InGame();
                    Client.ReturnButton.Visibility = Visibility.Hidden;
                    if (!Settings.Default.DisableClientSound)
                    {
                        Client.AmbientSoundPlayer.Stop();
                    }
                }));

                #endregion Launching Game
            }
            else if (message is TradeContractDTO)
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
            Skins = null;
            Skins = new List<int>();
            champions champion = champions.GetChampion(selectedChampionId);
            if (champion == null)
                return;

            SkinSelectListView.Items.Clear();
            AbilityListView.Items.Clear();

            //Render default skin
            var defaultSkinItem = new ListViewItem();
            var skinImage = new Image();
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champion.portraitPath)))
            {
                string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champion.portraitPath);
                skinImage.Source = Client.GetImage(UriSource);
                skinImage.Width = 191;
                skinImage.Stretch = Stretch.UniformToFill;
                defaultSkinItem.Tag = "0:" + champion.id; //Hack
                defaultSkinItem.Content = skinImage;
                SkinSelectListView.Items.Add(defaultSkinItem);
                //Render abilities
                List<championAbilities> Abilities = championAbilities.GetAbilities(selectedChampionId);
                var abilities = new List<ChampionAbility>();
                foreach (championAbilities ability in Abilities)
                {
                    var championAbility = new ChampionAbility();
                    UriSource = Path.Combine(Client.ExecutingDirectory, "Assets",
                        ability.iconPath.ToLower().Contains("passive") ? "passive" : "spell", ability.iconPath);
                    championAbility.AbilityImage.Source = Client.GetImage(UriSource);
                    if (!string.IsNullOrEmpty(ability.hotkey))
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
                    if ((DateTime.Now.Month == 4) && (DateTime.Now.Day == 1))
                        championAbility.AbilityDescription.Text = ability.description.Replace("bard", "tard").Replace("Bard", "Tard");
                    else
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
                    ListViewItem skinItem = new ListViewItem();
                    skinImage = new Image();
                    UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                        championSkins.GetSkin(skin.SkinId).portraitPath);
                    skinImage.Source = Client.GetImage(UriSource);
                    skinImage.Width = 191;
                    skinImage.Stretch = Stretch.UniformToFill;
                    skinItem.Tag = skin.SkinId;
                    skinItem.Content = skinImage;
                    SkinSelectListView.Items.Add(skinItem);
                    Skins.Add(skin.SkinId);
                }

                if (Skins.Count > 0)
                {
                    int index = SkinSelectListView.Items.IndexOf((ListViewItem)defaultSkinItem);
                    ListViewItem randomSkinItem = new ListViewItem();
                    Image randomSkinImage = new Image();
                    var src = "/LegendaryClient;component/NONE.png";
                    randomSkinImage.Source = Client.GetImage(src);
                    randomSkinImage.Width = 191;
                    randomSkinImage.Stretch = Stretch.UniformToFill;
                    randomSkinItem.Tag = "random";
                    randomSkinItem.Content = randomSkinImage;
                    SkinSelectListView.Items.Insert(index, randomSkinItem);
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
                                case "ShUrima":
                                    if (!shUrimaChampions.Contains(getChamp.displayName)) continue;
                                    break;
                                case "Discord":
                                    if (!discordChampions.Contains(getChamp.displayName)) continue;
                                    break;
                            }
                        }
                    }
                    if (((!champ.Owned && !champ.FreeToPlay) ||
                        !getChamp.displayName.ToLower().Contains(SearchTextBox.Text.ToLower())) &&
                        LatestDto.QueueTypeName != "COUNTER_PICK")
                        continue;

                    //Add to ListView
                    var item = new ListViewItem();
                    var championImage = new ChampionImage
                    {
                        ChampImage = { Source = champions.GetChampion(champ.ChampionId).icon }
                    };

                    if (champ.FreeToPlay || !champ.Active)
                        championImage.FreeToPlayLabel.Visibility = Visibility.Visible;

                    if (!champ.Active)
                    {
                        disabledCharacters.Add(champ.ChampionId);
                        championImage.FreeToPlayLabel.Content = "Disabled";
                        championImage.FreeToPlayLabel.FontSize = 11;
                    }

                    championImage.Width = 60;
                    championImage.Height = 60;
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
                        Width = 60,
                        Height = 60
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
            var control = new ChampSelectPlayer(selection.SummonerInternalName, selection.ChampionId, true);
            //Render champion
            if (selection.ChampionId != 0)
            {
                control.ChampionImage.Source = champions.GetChampion(selection.ChampionId).icon;
                control.Tag = selection.ChampionId;
            }

            //Render summoner spells
            if (selection.Spell1Id != 0)
            {
                string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                    SummonerSpell.GetSpellImageName((int)selection.Spell1Id));
                control.SummonerSpell1.Source = Client.GetImage(UriSource);
                UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                    SummonerSpell.GetSpellImageName((int)selection.Spell2Id));
                control.SummonerSpell2.Source = Client.GetImage(UriSource);
            }
            //Set our summoner spells in client
            if (player.SummonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
            {
                string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                    SummonerSpell.GetSpellImageName((int)selection.Spell1Id));
                SummonerSpell1Image.Source = Client.GetImage(UriSource);
                UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell",
                    SummonerSpell.GetSpellImageName((int)selection.Spell2Id));
                SummonerSpell2Image.Source = Client.GetImage(UriSource);
                MyChampId = selection.ChampionId;
            }
            //Has locked in
            if (player.PickMode == 2)
            {
                const string UriSource = "/LegendaryClient;component/Locked.png";
                control.LockedInIcon.Source = Client.GetImage(UriSource);
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
                //byte hack to get individual hex colors
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
            await RiotCalls.AttemptTrade(p.Value.SummonerInternalName, p.Key.ChampionId);

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
                //SelectChampion.SelectChampion(selection.ChampionId)*/
                await RiotCalls.SelectChampion(SelectChampion.SelectChamp((int)item.Tag));
                CanLockIn = true;
                Client.ChampId = (int)item.Tag;
                //TODO: Fix stupid animation glitch on left hand side
                var fadingAnimation = new DoubleAnimation
                {
                    From = 0.4,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.2))
                };
                fadingAnimation.Completed += (eSender, eArgs) =>
                {
                    string UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                        champions.GetChampion((int)item.Tag).splashPath);
                    BackgroundSplash.Source = Client.GetImage(UriSource);
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
            {
                SearchTextBox.Text = string.Empty;
                await RiotCalls.BanChampion((int)item.Tag);
            }
        }

        private async void SkinSelectListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item == null)
                return;

            if (item.Tag == null)
                return;

            var s = item.Tag as string;
            if (s!= null && s == "random")
            {
                int index = new Random().Next(0, Skins.Count);
                int skinId = Skins[index];
                championSkins skin = championSkins.GetSkin(skinId);
                await RiotCalls.SelectChampionSkin(skin.championId, skin.id);
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = "Selected a random skin" + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                if (Client.Dev) Client.Log("Selected " + skin.displayName + " as skin");
            }
            else if (s != null)
            {
                string[] splitItem = s.Split(':');
                int championId = Convert.ToInt32(splitItem[1]);
                champions champion = champions.GetChampion(championId);
                await RiotCalls.SelectChampionSkin(championId, 0);
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = "Selected Default " + champion.name + " as skin" + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }
            else
            {
                championSkins skin = championSkins.GetSkin((int)item.Tag);
                await RiotCalls.SelectChampionSkin(skin.championId, skin.id);
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
            if (Settings.Default.warnClose)
            {
                Warning Warn = new Warning();
                Warn.Header.Content = "Dodge Game";
                Warn.MessageText.Text = "Are You Sure You Want To Dodge? You will still have to spend the time penalty";
                Warn.ReturnButton.Click += (o, m) =>
                {
                    Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
                };
                Warn.ExitButton.Click += (o, m) =>
                {
                    QuitCurrentGame();
                    Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
                };
                Warn.HideButton.Click += (o, m) =>
                {
                    Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
                };
                Warn.ReturnButton.Content = "Return to Champ Select";
                Warn.ExitButton.Content = "Dodge game";
                Client.FullNotificationOverlayContainer.Content = Warn.Content;
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
            }
            else
            {
                QuitCurrentGame();
            }
        }

        private async void QuitCurrentGame()
        {
            Client.AmbientSoundPlayer.Stop();
            await RiotCalls.QuitGame();
            Client.RiotConnection.MessageReceived -= ChampSelect_OnMessageReceived;
            Client.ClearPage(typeof(CustomGameLobbyPage));
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.GameStatus = "outOfGame";
            Client.SetChatHover();
            Client.ReturnButton.Visibility = Visibility.Hidden;
            Client.SwitchPage(Client.MainPage);
            Client.ClearPage(typeof(ChampSelectPage));
        }

        private async void InGame()
        {
            await RiotCalls.QuitGame();
            Client.RiotConnection.MessageReceived -= ChampSelect_OnMessageReceived;
            Client.ClearPage(typeof(CustomGameLobbyPage));
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.ClearPage(typeof(FactionsCreateGamePage));
            Client.GameStatus = "inGame";
            Client.timeStampSince =
                (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalMilliseconds;
            Client.SetChatHover();

            Client.SwitchPage(new InGame(true));
            Client.ClearPage(typeof(ChampSelectPage));
        }

        private async void LockInButton_Click(object sender, RoutedEventArgs e)
        {
            if (configType.PickMode != "AllRandomPickStrategy")
            {
                if (!CanLockIn)
                    return;

                await RiotCalls.ChampionSelectCompleted();
                HasLockedIn = true;
                CanLockIn = false;
                this.LockInButton.IsEnabled = false;
                this.LockInButton.Background = Brushes.Gray;
            }
            else
            {
                await RiotCalls.Roll();
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
                await RiotCalls.SaveMasteryBook(bookDto);
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
                await RiotCalls.SelectDefaultSpellBookPage(selectedRunePage);
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
                Client.XmppConnection.Send(new Message(jid, MessageType.chat, ChatTextBox.Text));
                ChatTextBox.Text = "";
                ChatText.ScrollToEnd();
            }
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

        private async void LocalRuneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!QuickLoad) //Make loading quicker
                return;

            int i = 0;
            string[] runeIds = Client.LocalRunePages[LocalRuneComboBox.SelectedItem.ToString()].Split(',');

            var failsafe = Client.LoginPacket.AllSummonerData.SpellBook;
            try
            {
                foreach (string item in runeIds)
                {
                    Client.LoginPacket.AllSummonerData.SpellBook.BookPages[RuneComboBox.SelectedIndex].SlotEntries[i].RuneId = Convert.ToInt32(item);
                    i++;
                }


                if ((await RiotCalls.SaveSpellBook(Client.LoginPacket.AllSummonerData.SpellBook)).DefaultPage == null)
                {
                    Client.LoginPacket.AllSummonerData.SpellBook = failsafe;
                    var pop = new NotifyPlayerPopup("Save failed", "Failed to use local rune page.")
                    {
                        HorizontalAlignment = HorizontalAlignment.Right,
                        VerticalAlignment = VerticalAlignment.Bottom
                    };
                    Client.NotificationGrid.Children.Add(pop);
                }
            }
            catch
            {
                Client.LoginPacket.AllSummonerData.SpellBook = failsafe;
                var pop = new NotifyPlayerPopup("Save failed", "Failed to use local rune page.")
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                Client.NotificationGrid.Children.Add(pop);
            }
        }
    }
}
