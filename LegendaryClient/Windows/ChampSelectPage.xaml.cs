using jabber.connection;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SoundLogic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Reroll.Pojo;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;
using PVPNetConnect.RiotObjects.Platform.Trade;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ChampSelectPage.xaml
    /// </summary>
    public partial class ChampSelectPage : Page
    {
        private bool _BanningPhase;
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

        private int _LastPickTurn;
        private int LastPickTurn
        {
            get { return _LastPickTurn; }
            set
            {
                if (_LastPickTurn != value)
                {
                    counter = configType.MainPickTimerDuration - 3;
                    _LastPickTurn = value;
                }
            }
        }

        private double _MyChampId;
        private double MyChampId
        {
            get { return _MyChampId; }
            set
            {
                if (_MyChampId != value)
                {
                    PlayerTradeControl.Visibility = System.Windows.Visibility.Hidden;
                    _MyChampId = value;
                }
            }
        }

        private GameDTO LatestDto;
        private List<ChampionDTO> ChampList;
        private List<ChampionBanInfoDTO> ChampionsForBan;
        private MasteryBookDTO MyMasteries;
        private SpellBookDTO MyRunes;
        private PotentialTradersDTO CanTradeWith;
        private GameTypeConfigDTO configType;
        private System.Windows.Forms.Timer CountdownTimer;
        private int counter;
        private bool HasLockedIn = false;
        private bool DevMode = false;
        private bool HasLaunchedGame = false;
        private bool QuickLoad = false; //Don't load masteries and runes on load at start
        private Room Chatroom;
        private Page previousPage;
        private string[] bilgewaterChampions = { "Fizz", "Gangplank", "Graves", "Katarina", "Miss Fortune", "Nami", "Nautilus", "Twisted Fate" };
        private string[] freljordChampions = { "Anivia", "Ashe", "Braum", "Lissandra", "Nunu", "Sejuani", "Tryndamere", "Volibear", "Gragas" };
        private string[] ioniaChampions = { "Akali", "Irelia", "Karma", "Kennen", "Lee Sin", "Master Yi", "Shen", "Soraka", "Varus", "Ahri" };
        private string[] shadowIslesChampions = { "Elise", "Evelynn", "Fiddlesticks", "Hecarim", "Karthus", "Mordekaiser", "Nocturne", "Thresh", "Yorick", "Urgot" };
        private string[] demaciaChampions = { "Fiora", "Galio", "Garen", "Jarvan IV", "Leona", "Lucian", "Lux", "Poppy", "Quinn", "Shyvana", "Sona", "Vayne", "Xin Zhao" };
        private string[] noxusChampions = { "Annie", "Cassiopeia", "Darius", "Dr. Mundo", "Draven", "Katarina", "LeBlanc", "Morgana", "Singed", "Sion", "Sivir", "Swain", "Talon", "Urgot", "Vladimir", "Warwick" };
        private string[] piltoverChampions = { "Blitzcrank", "Caitlyn", "Corki", "Ezreal", "Heimerdinger", "Janna", "Jayce", "Jinx", "Orianna", "Vi", "Zac", "Ziggs", "Zilean" };
        private string[] bandleCityChampions = { "Amumu", "Corki", "Heimerdinger", "Kennen", "Lulu", "Poppy", "Rumble", "Teemo", "Tristana", "Veigar", "Ziggs" };
        private string[] zaunChampions = { "Blitzcrank", "Dr. Mundo", "Janna", "Jinx", "Renekton", "Singed", "Twisted Fate", "Twitch", "Urgot", "Viktor", "Warwick", "Zac" };
        private string[] voidChampions = { "Cho'Gath", "Kha'Zix", "Kog'Maw", "Malzahar", "Vel'koz" };
        private string[] shurimaChampions = { "Amumu", "Malzahar", "Nasus", "Renekton", "Sivir", "Skarner", "Xerath", "Zilean", "Azir" };
        private string[] discordChampions = { "Aatrox", "Cho'Gath", "Karthus", "Kha'Zix", "Kog'Maw", "Malzahar", "Nocturne", "Shaco", "Thresh", "Vel'koz" };
        bool AreWePurpleSide = false;

        public ChampSelectPage(Page previousPage)
        {
            InitializeComponent();
            StartChampSelect();
            this.previousPage = previousPage;
            string Sound = AmbientChampSelect.currentQueueToSoundFile(Client.QueueId);
            AmbientChampSelect.PlayAmbientChampSelectSound(Sound);
            Client.LastPageContent = this.Content;
            Client.runonce = true;

            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Champion Select";
        }

        /// <summary>
        /// Initializes all data required for champion select. Also retrieves latest GameDTO
        /// </summary>
        /// 

        /// <summary>
        /// Fix Champ Select
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

        internal static object LobbyContent = new object();

        private async void StartChampSelect()
        {
            //Force client to popup once in champion select
            Client.FocusClient();
            Client.IsInGame = true;
            //Get champions and sort alphabetically

            ChampList = new List<ChampionDTO>(Client.PlayerChampions);
            ChampList.Sort((x, y) => champions.GetChampion(x.ChampionId).displayName.CompareTo(champions.GetChampion(y.ChampionId).displayName));

            //Retrieve masteries and runes
            MyMasteries = Client.LoginPacket.AllSummonerData.MasteryBook;
            MyRunes = Client.LoginPacket.AllSummonerData.SpellBook;

            //Put masteries & runes into combo boxes
            int i = 0;
            foreach (MasteryBookPageDTO MasteryPage in MyMasteries.BookPages)
            {
                string MasteryPageName = MasteryPage.Name;
                //Stop garbage mastery names
                if (MasteryPageName.StartsWith("@@"))
                {
                    MasteryPageName = "Mastery Page " + ++i;
                }
                MasteryComboBox.Items.Add(MasteryPageName);
                if (MasteryPage.Current)
                    MasteryComboBox.SelectedValue = MasteryPageName;
            }
            i = 0;
            foreach (SpellBookPageDTO RunePage in MyRunes.BookPages)
            {
                string RunePageName = RunePage.Name;
                //Stop garbage rune names
                if (RunePageName.StartsWith("@@"))
                {
                    RunePageName = "Rune Page " + ++i;
                }
                RuneComboBox.Items.Add(RunePageName);
                if (RunePage.Current)
                    RuneComboBox.SelectedValue = RunePageName;
            }
            //Allow runes & masteries to be changed
            QuickLoad = true;

            //Signal to the server we are in champion select
            await Client.PVPNet.SetClientReceivedGameMessage(Client.GameID, "CHAMP_SELECT_CLIENT");
            //Retrieve the latest GameDTO
            GameDTO latestDTO = await Client.PVPNet.GetLatestGameTimerState(Client.GameID, Client.ChampSelectDTO.GameState, Client.ChampSelectDTO.PickTurn);
            //Find the game config for timers
            configType = Client.LoginPacket.GameTypeConfigs.Find(x => x.Id == latestDTO.GameTypeConfigId);
            if (configType == null) //Invalid config... abort!
            {
                QuitCurrentGame();

                MessageOverlay overlay = new MessageOverlay();
                overlay.MessageTextBox.Text = "Invalid Config ID (" + latestDTO.GameTypeConfigId.ToString() + "). Report to Eddy5641 [https://github.com/Eddy5641/LegendaryClient/issues/new]";
                overlay.MessageTitle.Content = "Invalid Config";
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;

            }
            else
            {
                counter = configType.MainPickTimerDuration - 5; //Seems to be a 5 second inconsistancy with riot and what they actually provide
                CountdownTimer = new System.Windows.Forms.Timer();
                CountdownTimer.Tick += new EventHandler(CountdownTimer_Tick);
                CountdownTimer.Interval = 1000; // 1 second
                CountdownTimer.Start();

                LatestDto = latestDTO;
                //Get the champions for the other team to ban & sort alpabetically
                ChampionBanInfoDTO[] ChampsForBan = await Client.PVPNet.GetChampionsForBan();
                ChampionsForBan = new List<ChampionBanInfoDTO>(ChampsForBan);
                ChampionsForBan.Sort((x, y) => champions.GetChampion(x.ChampionId).displayName.CompareTo(champions.GetChampion(y.ChampionId).displayName));

                //Join champion select chatroom
                string JID = Client.GetChatroomJID(latestDTO.RoomName.Replace("@sec", ""), latestDTO.RoomPassword, false);
                Chatroom = Client.ConfManager.GetRoom(new jabber.JID(JID));
                Chatroom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
                Chatroom.OnRoomMessage += Chatroom_OnRoomMessage;
                Chatroom.OnParticipantJoin += Chatroom_OnParticipantJoin;
                Chatroom.Join(latestDTO.RoomPassword);

                //Render our champions
                RenderChamps(false);

                //Start recieving champ select
                ChampSelect_OnMessageReceived(this, latestDTO);
                Client.OnFixChampSelect += ChampSelect_OnMessageReceived;
                Client.PVPNet.OnMessageReceived += ChampSelect_OnMessageReceived;
            }
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            counter--;
            if (counter <= 0)
            {
                return;
            }
                
            LobbyTimeLabel.Content = counter;
        }

        /// <summary>
        /// Main logic behind Champion Select
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="message"></param>
        private void ChampSelect_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                #region In Champion Select
                GameDTO ChampDTO = message as GameDTO;
                LatestDto = ChampDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    //Allow all champions to be selected (reset our modifications)
                    ListViewItem[] ChampionArray = new ListViewItem[ChampionSelectListView.Items.Count];
                    ChampionSelectListView.Items.CopyTo(ChampionArray, 0);
                    foreach (ListViewItem y in ChampionArray)
                    {
                        y.IsHitTestVisible = true;
                        y.Opacity = 1;
                    }

                    //Push all teams into one array to save a foreach call (looks messy)
                    List<Participant> AllParticipants = new List<Participant>(ChampDTO.TeamOne.ToArray());
                    AllParticipants.AddRange(ChampDTO.TeamTwo);
                    foreach (Participant p in AllParticipants)
                    {
                        if (p is PlayerParticipant)
                        {
                            PlayerParticipant play = (PlayerParticipant)p;
                            //If it is our turn to pick
                            if (play.PickTurn == ChampDTO.PickTurn)
                            {
                                if (play.SummonerId == Client.LoginPacket.AllSummonerData.Summoner.SumId)
                                {
                                    ChampionSelectListView.IsHitTestVisible = true;
                                    ChampionSelectListView.Opacity = 1;
                                    GameStatusLabel.Content = "Your turn to pick!";
                                    break;
                                }
                            }
                        }
                        //Otherwise block selection of champions unless in dev mode
                        if (!DevMode)
                        {
                            ChampionSelectListView.IsHitTestVisible = false;
                            ChampionSelectListView.Opacity = 0.5;
                        }
                        GameStatusLabel.Content = "Waiting for others to pick...";
                    }

                    //Champion select was cancelled 
                    if (ChampDTO.GameState == "TEAM_SELECT")
                    {
                        if (CountdownTimer != null)
                        {
                            CountdownTimer.Stop();
                        }
                        FixChampSelect();
                        FakePage fakePage = new FakePage();
                        fakePage.Content = LobbyContent;
                        Client.SwitchPage(fakePage);
                        return;
                    }
                    else if (ChampDTO.GameState == "PRE_CHAMP_SELECT")
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
                        foreach (var x in ChampDTO.BannedChampions)
                        {
                            Image champImage = new Image();
                            champImage.Height = 58;
                            champImage.Width = 58;
                            champImage.Source = champions.GetChampion(x.ChampionId).icon;
                            if (x.TeamId == 100)
                            {
                                BlueBanListView.Items.Add(champImage);
                            }
                            else
                            {
                                PurpleBanListView.Items.Add(champImage);
                            }

                            foreach (ListViewItem y in ChampionArray)
                            {
                                if ((int)y.Tag == x.ChampionId)
                                {
                                    ChampionSelectListView.Items.Remove(y);
                                    //Remove from arrays
                                    foreach (ChampionDTO PlayerChamps in ChampList.ToArray())
                                    {
                                        if (x.ChampionId == PlayerChamps.ChampionId)
                                        {
                                            ChampList.Remove(PlayerChamps);
                                            break;
                                        }
                                    }

                                    foreach (ChampionBanInfoDTO BanChamps in ChampionsForBan.ToArray())
                                    {
                                        if (x.ChampionId == BanChamps.ChampionId)
                                        {
                                            ChampionsForBan.Remove(BanChamps);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        #endregion Render Bans
                    }
                    else if (ChampDTO.GameState == "CHAMP_SELECT")
                    {
                        //Picking has started. If pickturn has changed reset timer
                        LastPickTurn = ChampDTO.PickTurn;
                        BanningPhase = false;
                    }
                    else if (ChampDTO.GameState == "POST_CHAMP_SELECT")
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
                    else if (ChampDTO.GameState == "START_REQUESTED")
                    {
                        GameStatusLabel.Content = "The game is about to start!";
                        DodgeButton.IsEnabled = false; //Cannot dodge past this point!
                        counter = 1;
                    }
                    else if (ChampDTO.GameState == "TERMINATED")
                    {
                        NotifyPlayerPopup pop = new NotifyPlayerPopup("Player Dodged", "Player has Dodged Queue.");
                        pop.HorizontalAlignment = HorizontalAlignment.Right;
                        pop.VerticalAlignment = VerticalAlignment.Bottom;
                        Client.NotificationGrid.Children.Add(pop);
                        Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
                        Client.OnFixChampSelect -= ChampSelect_OnMessageReceived;
                        if (previousPage is TeamQueuePage)
                            (previousPage as TeamQueuePage).readdHandler();
                        else if (previousPage is PlayPage)
                            (previousPage as PlayPage).readdHandler();
                        Client.SwitchPage(previousPage);
                        Client.ClearPage(typeof(ChampSelectPage));
                        Client.ReturnButton.Visibility = Visibility.Hidden;
                    }

                    #region Display players

                    BlueListView.Items.Clear();
                    PurpleListView.Items.Clear();
                    int i = 0;
                    bool PurpleSide = false;

                    //Aram hack, view other players champions & names (thanks to Andrew)
                    List<PlayerChampionSelectionDTO> OtherPlayers = new List<PlayerChampionSelectionDTO>(ChampDTO.PlayerChampionSelections.ToArray());
                    AreWePurpleSide = false;

                    foreach (Participant participant in AllParticipants)
                    {
                        Participant tempParticipant = participant;
                        i++;
                        ChampSelectPlayer control = new ChampSelectPlayer();
                        //Cast AramPlayers as PlayerParticipants. This removes reroll data
                        if (tempParticipant is AramPlayerParticipant)
                        {
                            tempParticipant = new PlayerParticipant(tempParticipant.GetBaseTypedObject());
                        }

                        if (tempParticipant is PlayerParticipant)
                        {
                            PlayerParticipant player = tempParticipant as PlayerParticipant;
                            if (!String.IsNullOrEmpty(player.SummonerName))
                                control.PlayerName.Content = player.SummonerName;
                            else
                            {
                                AllPublicSummonerDataDTO Summoner = await Client.PVPNet.GetAllPublicSummonerDataByAccount(player.SummonerId);
                                if (!String.IsNullOrEmpty(Summoner.Summoner.Name))
                                    control.PlayerName.Content = Summoner.Summoner.Name;
                                else
                                    control.PlayerName.Content = "Unknown Player";
                            }

                            foreach (PlayerChampionSelectionDTO selection in ChampDTO.PlayerChampionSelections)
                            {
                                #region Disable picking selected champs

                                foreach (ListViewItem y in ChampionArray)
                                {
                                    if ((int)y.Tag == selection.ChampionId)
                                    {
                                        y.IsHitTestVisible = true;
                                        y.Opacity = 0.5;
                                        if (configType != null)
                                        {
                                            if (configType.DuplicatePick)
                                            {
                                                y.IsHitTestVisible = false;
                                                y.Opacity = 1;
                                            }
                                        }
                                    }
                                }

                                #endregion Disable picking selected champs

                                if (selection.SummonerInternalName == player.SummonerInternalName)
                                {
                                    //Clear our teams champion selection for aram hack
                                    OtherPlayers.Remove(selection);
                                    control = RenderPlayer(selection, player);
                                    //If we have locked in render skin select
                                    if (HasLockedIn && selection.SummonerInternalName == Client.LoginPacket.AllSummonerData.Summoner.InternalName && !DevMode)
                                    {
                                        if (PurpleSide)
                                            AreWePurpleSide = true;
                                        RenderLockInGrid(selection);
                                        if (player.PointSummary != null)
                                        {
                                            LockInButton.Content = string.Format("Reroll ({0}/{1})", player.PointSummary.CurrentPoints, player.PointSummary.PointsCostToRoll);
                                            if (player.PointSummary.NumberOfRolls > 0)
                                                LockInButton.IsEnabled = true;
                                            else
                                                LockInButton.IsEnabled = false;
                                        }
                                    }
                                }
                            }
                        }
                        else if (tempParticipant is ObfuscatedParticipant)
                        {
                            control.PlayerName.Content = "Summoner " + i;
                        }
                        else if (tempParticipant is BotParticipant)
                        {
                            BotParticipant bot = tempParticipant as BotParticipant;
                            string botChamp = bot.SummonerName.Split(' ')[0]; //Why is this internal name rito?
                            champions botSelectedChamp = champions.GetChampion(botChamp);
                            PlayerParticipant part = new PlayerParticipant();
                            PlayerChampionSelectionDTO selection = new PlayerChampionSelectionDTO();
                            selection.ChampionId = botSelectedChamp.id;
                            part.SummonerName = botSelectedChamp.displayName + " bot";
                            control = RenderPlayer(selection, part);
                        }
                        else
                        {
                            control.PlayerName.Content = "Unknown Summoner";
                        }
                        //Display purple side if we have gone through our team
                        if (i > ChampDTO.TeamOne.Count)
                        {
                            i = 0;
                            PurpleSide = true;
                        }

                        if (!PurpleSide)
                        {
                            BlueListView.Items.Add(control);
                        }
                        else
                        {
                            PurpleListView.Items.Add(control);
                        }
                    }

                    //Do aram hack!
                    if (OtherPlayers.Count > 0)
                    {
                        if (AreWePurpleSide)
                        {
                            BlueListView.Items.Clear();
                        }
                        else
                        {
                            PurpleListView.Items.Clear();
                        }

                        foreach (PlayerChampionSelectionDTO hackSelection in OtherPlayers)
                        {
                            ChampSelectPlayer control = new ChampSelectPlayer();
                            PlayerParticipant player = new PlayerParticipant();
                            player.SummonerName = hackSelection.SummonerInternalName;
                            control = RenderPlayer(hackSelection, player);

                            if (AreWePurpleSide)
                            {
                                BlueListView.Items.Add(control);
                            }
                            else
                            {
                                PurpleListView.Items.Add(control);
                            }
                        }
                    }

                    #endregion Display players
                }));

                #endregion In Champion Select
            }
            else if (message.GetType() == typeof(PlayerCredentialsDto))
            {
                #region Launching Game

                PlayerCredentialsDto dto = message as PlayerCredentialsDto;
                Client.CurrentGame = dto;

                if (!HasLaunchedGame)
                {
                    HasLaunchedGame = true;
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        if (CountdownTimer != null)
                        {
                            CountdownTimer.Stop();
                        }
                        Client.LaunchGame();
                        InGame();
                        Client.ReturnButton.Visibility = Visibility.Hidden;
                    }));
                }

                #endregion Launching Game
            }
            else if (message.GetType() == typeof(TradeContractDTO))
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    TradeContractDTO TradeDTO = message as TradeContractDTO;
                    if (TradeDTO.State == "PENDING")
                    {
                        PlayerTradeControl.Visibility = System.Windows.Visibility.Visible;
                        PlayerTradeControl.Tag = TradeDTO;
                        PlayerTradeControl.AcceptButton.Visibility = System.Windows.Visibility.Visible;
                        PlayerTradeControl.DeclineButton.Content = "Decline";

                        champions MyChampion = champions.GetChampion((int)TradeDTO.ResponderChampionId);
                        PlayerTradeControl.MyChampImage.Source = MyChampion.icon;
                        PlayerTradeControl.MyChampLabel.Content = MyChampion.displayName;
                        champions TheirChampion = champions.GetChampion((int)TradeDTO.RequesterChampionId);
                        PlayerTradeControl.TheirChampImage.Source = TheirChampion.icon;
                        PlayerTradeControl.TheirChampLabel.Content = TheirChampion.displayName;
                        PlayerTradeControl.RequestLabel.Content = string.Format("{0} wants to trade!", TradeDTO.RequesterInternalSummonerName);
                    }
                    else if (TradeDTO.State == "CANCELED" || TradeDTO.State == "DECLINED" || TradeDTO.State == "BUSY")
                    {
                        PlayerTradeControl.Visibility = System.Windows.Visibility.Hidden;

                        TextInfo Text = new CultureInfo("en-US", false).TextInfo;
                        NotificationPopup pop = new NotificationPopup(ChatSubjects.INVITE_STATUS_CHANGED,
                            string.Format("{0} has {1} this trade", TradeDTO.RequesterInternalSummonerName, Text.ToTitleCase(TradeDTO.State)));

                        if (TradeDTO.State == "BUSY")
                            pop.NotificationTextBox.Text = string.Format("{0} is currently busy", TradeDTO.RequesterInternalSummonerName);

                        pop.Height = 200;
                        pop.OkButton.Visibility = System.Windows.Visibility.Visible;
                        pop.HorizontalAlignment = HorizontalAlignment.Right;
                        pop.VerticalAlignment = VerticalAlignment.Bottom;
                        Client.NotificationGrid.Children.Add(pop);//*/
                    }
                }));
            }
        }

        /// <summary>
        /// Render the post selection grid after locked in
        /// </summary>
        /// <param name="selection">Details of champion you want to render</param>
        internal void RenderLockInGrid(PlayerChampionSelectionDTO selection)
        {
            ChampionSelectListView.Visibility = Visibility.Hidden;
            AfterChampionSelectGrid.Visibility = Visibility.Visible;

            LockInButton.Content = "Locked In";

            //
            champions Champion = champions.GetChampion(selection.ChampionId);

            SkinSelectListView.Items.Clear();
            AbilityListView.Items.Clear();

            //Render default skin
            ListViewItem item = new ListViewItem();
            Image skinImage = new Image();
            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Champion.portraitPath);
            skinImage.Source = Client.GetImage(uriSource);
            skinImage.Width = 191;
            skinImage.Stretch = Stretch.UniformToFill;
            item.Tag = "0:" + Champion.id; //Hack
            item.Content = skinImage;
            SkinSelectListView.Items.Add(item);

            //Render abilities
            List<championAbilities> Abilities = championAbilities.GetAbilities(selection.ChampionId);
            foreach (championAbilities ability in Abilities)
            {
                ChampionAbility championAbility = new ChampionAbility();
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "abilities", ability.iconPath);
                championAbility.AbilityImage.Source = Client.GetImage(uriSource);
                championAbility.AbilityHotKey.Content = ability.hotkey;
                championAbility.AbilityName.Content = ability.name;
                championAbility.AbilityDescription.Text = ability.description;
                championAbility.Width = 375;
                championAbility.Height = 75;
                AbilityListView.Items.Add(championAbility);
            }

            //Render champions
            foreach (ChampionDTO champ in ChampList)
            {
                if (champ.ChampionId == selection.ChampionId)
                {
                    foreach (ChampionSkinDTO skin in champ.ChampionSkins)
                    {
                        if (skin.Owned)
                        {
                            item = new ListViewItem();
                            skinImage = new Image();
                            uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", championSkins.GetSkin(skin.SkinId).portraitPath);
                            skinImage.Source = Client.GetImage(uriSource);
                            skinImage.Width = 191;
                            skinImage.Stretch = Stretch.UniformToFill;
                            item.Tag = skin.SkinId;
                            item.Content = skinImage;
                            SkinSelectListView.Items.Add(item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Render all champions
        /// </summary>
        /// <param name="RenderBans">Render champions for ban</param>
        internal void RenderChamps(bool RenderBans)
        {
            ChampionSelectListView.Items.Clear();
            if (!RenderBans)
            {
                foreach (ChampionDTO champ in ChampList)
                {
                    champions getChamp = champions.GetChampion(champ.ChampionId);
                    if (previousPage.GetType() == typeof(FactionsGameLobbyPage))
                    {
                        FactionsGameLobbyPage page = previousPage as FactionsGameLobbyPage;
                        LeftTeamLabel.Content = page.getLeftTeam();
                        RightTeamLabel.Content = page.getRightTeam();
                        string myTeam = (AreWePurpleSide) ? page.getRightTeam() : page.getLeftTeam();
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
                    if ((champ.Owned || champ.FreeToPlay) && getChamp.displayName.ToLower().Contains(SearchTextBox.Text.ToLower()))
                    {
                        //Add to ListView
                        ListViewItem item = new ListViewItem();
                        ChampionImage championImage = new ChampionImage();
                        championImage.ChampImage.Source = champions.GetChampion(champ.ChampionId).icon;
                        if (champ.FreeToPlay)
                            championImage.FreeToPlayLabel.Visibility = Visibility.Visible;
                        championImage.Width = 64;
                        championImage.Height = 64;
                        item.Tag = champ.ChampionId;
                        item.Content = championImage.Content;
                        ChampionSelectListView.Items.Add(item);
                    }
                }
            }
            else
            {
                foreach (ChampionBanInfoDTO champ in ChampionsForBan)
                {
                    champions getChamp = champions.GetChampion(champ.ChampionId);
                    if (champ.EnemyOwned && getChamp.displayName.ToLower().Contains(SearchTextBox.Text.ToLower()))
                    {
                        //Add to ListView
                        ListViewItem item = new ListViewItem();
                        ChampionImage championImage = new ChampionImage();
                        championImage.ChampImage.Source = champions.GetChampion(champ.ChampionId).icon;
                        championImage.Width = 64;
                        championImage.Height = 64;
                        item.Tag = champ.ChampionId;
                        item.Content = championImage.Content;
                        ChampionSelectListView.Items.Add(item);
                    }
                }
            }
        }

        /// <summary>
        /// Render individual players
        /// </summary>
        /// <param name="selection">The champion the player has selected</param>
        /// <param name="player">The participant details of the player</param>
        /// <returns></returns>
        internal ChampSelectPlayer RenderPlayer(PlayerChampionSelectionDTO selection, PlayerParticipant player)
        {
            ChampSelectPlayer control = new ChampSelectPlayer();
            //Render champion
            if (selection.ChampionId != 0)
            {
                control.ChampionImage.Source = champions.GetChampion(selection.ChampionId).icon;
            }
            //Render summoner spells
            if (selection.Spell1Id != 0)
            {
                string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)selection.Spell1Id));
                control.SummonerSpell1.Source = Client.GetImage(uriSource);
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)selection.Spell2Id));
                control.SummonerSpell2.Source = Client.GetImage(uriSource);
            }
            //Set our summoner spells in client
            if (player.SummonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
            {
                string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)selection.Spell1Id));
                SummonerSpell1Image.Source = Client.GetImage(uriSource);
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)selection.Spell2Id));
                SummonerSpell2Image.Source = Client.GetImage(uriSource);
                MyChampId = selection.ChampionId;
            }
            //Has locked in
            if (player.PickMode == 2)
            {
                string uriSource = "/LegendaryClient;component/Locked.png";
                control.LockedInIcon.Source = Client.GetImage(uriSource);
            }
            //Make obvious whos pick turn it is
            if (player.PickTurn != LatestDto.PickTurn && (LatestDto.GameState == "CHAMP_SELECT" || LatestDto.GameState == "PRE_CHAMP_SELECT"))
            {
                control.Opacity = 0.5;
            }
            else
            {
                //Full opacity when not picking or banning
                control.Opacity = 1;
            }
            //If trading with this player is possible
            if (CanTradeWith != null && (CanTradeWith.PotentialTraders.Contains(player.SummonerInternalName) || DevMode))
            {
                control.TradeButton.Visibility = System.Windows.Visibility.Visible;
            }
            //If this player is duo/trio/quadra queued with players
            if (player.TeamParticipantId != null && (double)player.TeamParticipantId != 0)
            {
                //Byte hack to get individual hex colors
                byte[] values = BitConverter.GetBytes((double)player.TeamParticipantId);
                if (!BitConverter.IsLittleEndian) Array.Reverse(values);

                byte r = values[2];
                byte b = values[3];
                byte g = values[4];

                System.Drawing.Color myColor = System.Drawing.Color.FromArgb(r, b, g);

                var converter = new System.Windows.Media.BrushConverter();
                var brush = (Brush)converter.ConvertFromString("#" + myColor.Name);
                control.TeamRectangle.Fill = brush;
                control.TeamRectangle.Visibility = System.Windows.Visibility.Visible;
            }
            control.LockedInIcon.Visibility = System.Windows.Visibility.Visible;
            control.TradeButton.Tag = new KeyValuePair<PlayerChampionSelectionDTO, PlayerParticipant>(selection, player);
            control.TradeButton.Click += TradeButton_Click;
            control.PlayerName.Content = player.SummonerName;
            return control;
        }

        private async void TradeButton_Click(object sender, RoutedEventArgs e)
        {
            KeyValuePair<PlayerChampionSelectionDTO, PlayerParticipant> p = (KeyValuePair<PlayerChampionSelectionDTO, PlayerParticipant>)((Button)sender).Tag;
            await Client.PVPNet.AttemptTrade(p.Value.SummonerInternalName, p.Key.ChampionId);

            PlayerTradeControl.Visibility = System.Windows.Visibility.Visible;
            champions MyChampion = champions.GetChampion((int)MyChampId);
            PlayerTradeControl.MyChampImage.Source = MyChampion.icon;
            PlayerTradeControl.MyChampLabel.Content = MyChampion.displayName;
            champions TheirChampion = champions.GetChampion((int)p.Key.ChampionId);
            PlayerTradeControl.TheirChampImage.Source = TheirChampion.icon;
            PlayerTradeControl.TheirChampLabel.Content = TheirChampion.displayName;
            PlayerTradeControl.RequestLabel.Content = "Sent trade request...";
            PlayerTradeControl.AcceptButton.Visibility = System.Windows.Visibility.Hidden;
            PlayerTradeControl.DeclineButton.Content = "Cancel";
        }

        private async void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                if (!BanningPhase)
                {
                    if (item.Tag != null)
                    {
                        //SelectChampion.SelectChampion(selection.ChampionId)
                        await Client.PVPNet.SelectChampion(SelectChampion.SelectChamp((int)item.Tag));

                        //TODO: Fix stupid animation glitch on left hand side
                        DoubleAnimation fadingAnimation = new DoubleAnimation();
                        fadingAnimation.From = 0.4;
                        fadingAnimation.To = 0;
                        fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
                        fadingAnimation.Completed += (eSender, eArgs) =>
                        {
                            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion((int)item.Tag).splashPath);
                            BackgroundSplash.Source = Client.GetImage(uriSource);
                            fadingAnimation = new DoubleAnimation();
                            fadingAnimation.From = 0;
                            fadingAnimation.To = 0.4;
                            fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

                            BackgroundSplash.BeginAnimation(Image.OpacityProperty, fadingAnimation);
                        };

                        BackgroundSplash.BeginAnimation(Image.OpacityProperty, fadingAnimation);
                    }
                }
                else
                {
                    if (item.Tag != null)
                    {
                        await Client.PVPNet.BanChampion((int)item.Tag);
                    }
                }
            }
        }

        private async void SkinSelectListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                if (item.Tag != null)
                {
                    if (item.Tag is string)
                    {
                        string[] splitItem = ((string)item.Tag).Split(':');
                        int championId = Convert.ToInt32(splitItem[1]);
                        champions Champion = champions.GetChampion(championId);
                        await Client.PVPNet.SelectChampionSkin(championId, 0);
                        TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                        tr.Text = "Selected Default " + Champion.name + " as skin" + Environment.NewLine;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    }
                    else
                    {
                        championSkins skin = championSkins.GetSkin((int)item.Tag);
                        await Client.PVPNet.SelectChampionSkin(skin.championId, skin.id);
                        TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                        tr.Text = "Selected " + skin.displayName + " as skin" + Environment.NewLine;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    }
                    ChatText.ScrollToEnd();
                }
            }
        }

        private void DodgeButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO - add messagebox
            Warning pop = new Warning();
            //pop.hide.Click =
            //Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
            Client.ReturnButton.Visibility = Visibility.Hidden;
            QuitCurrentGame();
        }

        private async void QuitCurrentGame()
        {
            await Client.PVPNet.QuitGame();
            Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
            Client.ClearPage(typeof(CustomGameLobbyPage));
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.ClearPage(typeof(ChampSelectPage));

            Client.SwitchPage(new MainPage());
        }

        private async void InGame()
        {
            await Client.PVPNet.QuitGame();
            Client.PVPNet.OnMessageReceived -= ChampSelect_OnMessageReceived;
            Client.ClearPage(typeof(CustomGameLobbyPage));
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.ClearPage(typeof(ChampSelectPage));
            Client.ClearPage(typeof(FactionsCreateGamePage));

            Client.SwitchPage(new InGame());
        }

        private async void LockInButton_Click(object sender, RoutedEventArgs e)
        {
            if (configType.PickMode != "AllRandomPickStrategy")
            {
                if (ChampionSelectListView.SelectedItems.Count > 0)
                {
                    await Client.PVPNet.ChampionSelectCompleted();
                    HasLockedIn = true;
                }
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

            bool HasChanged = false;
            int i = 0;
            MasteryBookDTO bookDTO = new MasteryBookDTO();
            bookDTO.SummonerId = Client.LoginPacket.AllSummonerData.Summoner.SumId;
            bookDTO.BookPages = new List<MasteryBookPageDTO>();
            foreach (MasteryBookPageDTO MasteryPage in MyMasteries.BookPages)
            {
                string MasteryPageName = MasteryPage.Name;
                //Convert garbage to readable so we get the proper mastery page
                if (MasteryPageName.StartsWith("@@"))
                {
                    MasteryPageName = "Mastery Page " + ++i;
                }
                MasteryPage.Current = false;
                if (MasteryPageName == (string)MasteryComboBox.SelectedItem)
                {
                    MasteryPage.Current = true;
                    HasChanged = true;
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = "Selected " + MasteryPageName + " as Mastery Page" + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    ChatText.ScrollToEnd();
                }
                bookDTO.BookPages.Add(MasteryPage);
            }
            if (HasChanged)
            {
                await Client.PVPNet.SaveMasteryBook(bookDTO);
            }
        }

        private async void RuneComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!QuickLoad) //Make loading quicker
                return;

            SpellBookPageDTO SelectedRunePage = new SpellBookPageDTO();
            int i = 0;
            bool HasChanged = false;
            foreach (SpellBookPageDTO RunePage in MyRunes.BookPages)
            {
                string RunePageName = RunePage.Name;
                if (RunePageName.StartsWith("@@"))
                {
                    RunePageName = "Rune Page " + ++i;
                }
                RunePage.Current = false;
                if (RunePageName == (string)RuneComboBox.SelectedItem)
                {
                    RunePage.Current = true;
                    SelectedRunePage = RunePage;
                    HasChanged = true;
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = "Selected " + RunePageName + " as Rune Page" + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    ChatText.ScrollToEnd();
                }
            }
            if (HasChanged)
            {
                await Client.PVPNet.SelectDefaultSpellBookPage(SelectedRunePage);
            }
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            //Enable dev mode if !~dev is typed in chat
            if (ChatTextBox.Text == "!~dev")
            {
                DevMode = !DevMode;
                ChampionSelectListView.IsHitTestVisible = true;
                ChampionSelectListView.Opacity = 1;
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = "DEV MODE: " + DevMode + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatTextBox.Text = "";
            }
            else
            {
                if (DevMode)
                {
                    if (ChatTextBox.Text == "!~champ")
                    {
                        ChampionSelectListView.Visibility = Visibility.Visible;
                        AfterChampionSelectGrid.Visibility = Visibility.Hidden;
                    }
                    else if (ChatTextBox.Text == "!~skin")
                    {
                        ChampionSelectListView.Visibility = Visibility.Hidden;
                        AfterChampionSelectGrid.Visibility = Visibility.Visible;
                    }
                    ChatTextBox.Text = "";
                    return;
                }
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = ChatTextBox.Text + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                Chatroom.PublicMessage(ChatTextBox.Text);
                ChatTextBox.Text = "";
                ChatText.ScrollToEnd();
            }
        }

        private void Chatroom_OnRoomMessage(object sender, jabber.protocol.client.Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                //Ignore the message that is always sent when joining
                if (msg.Body != "This room is not anonymous")
                {
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.From.Resource + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    ChatText.ScrollToEnd();
                }
            }));
        }

        private void Chatroom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatText.ScrollToEnd();
            }));
        }
    }
}