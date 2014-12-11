using jabber.connection;
using jabber.protocol.client;
using LegendaryClient.Controls.TeamBuilder;
using LegendaryClient.Logic;
using LegendaryClient.Logic.PlayerSpell;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook;
using PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook;
using PVPNetConnect.RiotObjects.Platform.ServiceProxy.Dispatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Timers;
using LegendaryClient.Controls;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using System.Globalization;
using PVPNetConnect.RiotObjects.Platform.Game;
using LegendaryClient.Logic.Replays;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for TeamBuilder.xaml
    /// </summary>
    public partial class TeamBuilderPage : Page
    {
        private Room newRoom;
        internal int ChampionId = 0;

        /// <summary>
        /// Possible roles:
        /// MAGE
        /// SUPPORT
        /// ASSASSIN
        /// MARKSMAN
        /// FIGHTER
        /// TANK
        /// </summary>
        internal string role;

        /// <summary>
        /// TOP
        /// MIDDLE
        /// BOTTOM
        /// JUNGLE
        /// </summary>
        internal string position;

        internal int skinId;

        internal bool connectedToChat = false;

        internal List<int> availableSpells = new List<int>();
        internal int spell1 = 0;
        internal int spell2 = 0;

        private List<ChampionDTO> ChampList;
        private MasteryBookDTO MyMasteries;
        private SpellBookDTO MyRunes;


        internal string teambuilderGroupId;
        internal int teambuilderSlotId;
        internal int teambuilderCandidateAutoQuitTimeout;

        private LobbyStatus CurrentLobby;
        private System.Timers.Timer timer;
        private long inQueueTimer;
        private bool HasLaunchedGame = false;

        //TeamBuilder is just a little insane. This code is very messy too. :P
        /* 
         Note by horato: This code is not messy, its ugly as fuck. If you don't want to suffer from serious brain damage and moral panic
         do not attempt to study it. But hey, it works (kind of). 
         I also suck at GUI and xaml, if you want to fix my shit then be my guest ;)
        */
        public TeamBuilderPage(bool iscreater, LobbyStatus myLobby)
        {
            InitializeComponent();
            if (iscreater == false)
            {
                Invite.IsEnabled = false;
            }
            CurrentLobby = myLobby;

            MyMasteries = Client.LoginPacket.AllSummonerData.MasteryBook;
            MyRunes = Client.LoginPacket.AllSummonerData.SpellBook;
            LoadStats();

            Client.InviteListView = InvitedPlayers;
            Client.PVPNet.OnMessageReceived += PVPNet_OnMessageReceived;
            Client.LastPageContent = this.Content;
            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to team builder";
            Client.GameStatus = "inTeamBuilder";
            Client.SetChatHover();
            AddPlayer();

            CallWithArgs(Guid.NewGuid().ToString(), "cap", "retrieveFeatureToggles", "{}");
            CallWithArgs(Guid.NewGuid().ToString(), "cap", "retrieveInfoV1", "{\"queueId\":61}");
        }

        /// <summary>
        /// Use this to connect to chat
        /// </summary>
        /// <param name="ChatJID"></param>
        /// <param name="Pass"></param>
        private void ConnectToChat(string ChatJID, string Pass)
        {
            string JID = Client.GetChatroomJID(ChatJID, Pass, false);
            newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
            newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            newRoom.OnRoomMessage += newRoom_OnRoomMessage;
            newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
            newRoom.Join(Pass);
            connectedToChat = true;
        }
        private void LeaveChat()
        {
            newRoom.Leave("Player Quit TeamBuilder Lobby");
            //We no longer want to receive messages from teambuilder chat lobby if we want to leave that team
            newRoom.OnRoomMessage -= newRoom_OnRoomMessage;
            newRoom.OnParticipantJoin -= newRoom_OnParticipantJoin;
        }

        private void PVPNet_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(LcdsServiceProxyResponse))
            {
                LcdsServiceProxyResponse ProxyResponse = message as LcdsServiceProxyResponse;
                HandleProxyResponse(ProxyResponse);
            }
            if (message.GetType() == typeof(GameDTO))
            {
                GameDTO ChampDTO = message as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    if (ChampDTO.GameState == "START_REQUESTED")
                    {
                        // TODO: add fancy notification of game starting
                        QuitButton.IsEnabled = false;
                    }
                }));
            }
            else if (message.GetType() == typeof(PlayerCredentialsDto))
            {
                #region Launching Game

                PlayerCredentialsDto dto = message as PlayerCredentialsDto;
                Client.CurrentGame = dto;

                if (!HasLaunchedGame)
                {
                    HasLaunchedGame = true;
                    if (Properties.Settings.Default.AutoRecordGames)
                    {
                        Dispatcher.InvokeAsync(async () =>
                        {
                            PlatformGameLifecycleDTO n = await Client.PVPNet.RetrieveInProgressSpectatorGameInfo(Client.LoginPacket.AllSummonerData.Summoner.Name);
                            if (n.GameName != null)
                            {
                                string IP = n.PlayerCredentials.ObserverServerIp + ":" + n.PlayerCredentials.ObserverServerPort;
                                string Key = n.PlayerCredentials.ObserverEncryptionKey;
                                int GameID = (Int32)n.PlayerCredentials.GameId;
                                new ReplayRecorder(IP, GameID, Client.Region.InternalName, Key);
                            }
                        });
                    }
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        Client.LaunchGame();
                        InGame();
                    }));
                }

                #endregion Launching Game
            }
        }

        /// <summary>
        /// Used to start queuing For SOLO [yay riot]
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartQueue(object sender, RoutedEventArgs e)
        {
            if (spell1 == 0 || spell2 == 0)
                return;
            string roleUp = string.Format(role.ToUpper());
            string posUp = string.Format(position.ToUpper());
            string Json = string.Format("\"skinId\":{0},\"position\":\"{1}\",\"role\":\"{2}\",\"championId\":{3},\"spell2Id\":{4},\"queueId\":61,\"spell1Id\":{5}", skinId, posUp, roleUp, ChampionId, spell2, spell1);
            CallWithArgs(Guid.NewGuid().ToString(), "cap", "createSoloQueryV4", "{" + Json + "}");
        }

        private System.Timers.Timer CountdownTimer;

        private int TimeLeft = 10;

        private void HandleProxyResponse(LcdsServiceProxyResponse Response)
        {
            System.Diagnostics.Debug.WriteLine(Response.MethodName);
            if (Response.MethodName == "infoRetrievedV1")
            {
                /*
                  TODO: there is alot of info yet to be processed like available champs, skins,
                  demand info (roles/positions) and something called cpr adjustment (no fucking idea)
                  but it can wait.
                */
                InfoRetrieved response = JsonConvert.DeserializeObject<InfoRetrieved>(Response.Payload);
                if (response.initialSpellIds.Length > 0)
                {
                    spell1 = response.initialSpellIds[0];
                    if (response.initialSpellIds.Length > 1)
                        spell2 = response.initialSpellIds[1];

                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        RenderLegenadryClientPlayerSumSpellIcons();
                    }));
                }
                if (response.spellIds.Length > 0)
                    availableSpells = response.spellIds.ToList<int>();
            }
            else if (Response.MethodName == "estimatedWaitTimeRetrievedV1")
            {
                //TODO: make use of response.estimatedWaitTime if there is any
                //ReceivedWaitTime response = JsonConvert.DeserializeObject<ReceivedWaitTime>(Response.Payload);
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    QueueButton.IsEnabled = true;
                }));
            }
            else if (Response.MethodName == "soloQueryCreatedV1")
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    QueueButton.IsEnabled = false;
                    QueueButton.Content = "Searching for team";
                    TeamPlayer.Role.IsEnabled = false;
                    TeamPlayer.Position.IsEnabled = false;
                    TeamPlayer.SummonerSpell1.IsEnabled = false;
                    TeamPlayer.SummonerSpell2.IsEnabled = false;
                    TeamPlayer.MasteryPage.IsEnabled = false;
                    TeamPlayer.RunePage.IsEnabled = false;
                    TeamPlayer.SelectChampion.IsEnabled = false;
                }));
                inQueueTimer = 0;

                timer = new System.Timers.Timer(1000);
                timer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
                {
                    inQueueTimer++;
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        TimeSpan ts = TimeSpan.FromSeconds(inQueueTimer);
                        QueueButton.Content = String.Format("Searching for team {0}:{1}", ts.Minutes, ts.Seconds < 10 ? "0" + ts.Seconds : "" + ts.Seconds);
                    }));
                });
                timer.AutoReset = true;
                timer.Start();

            }
            else if (Response.MethodName == "acceptedByGroupV2")
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    timer.Stop();
                    TimeLeft = 10;
                    ReceivedGroupId m = JsonConvert.DeserializeObject<ReceivedGroupId>(Response.Payload);
                    teambuilderCandidateAutoQuitTimeout = m.candidateAutoQuitTimeout;
                    teambuilderGroupId = m.groupId;
                    teambuilderSlotId = m.slotId;
                    MatchFoundGrid.Visibility = Visibility.Visible;

                    TimeLeft = teambuilderCandidateAutoQuitTimeout;

                    CountdownTimer = new System.Timers.Timer(1000);
                    CountdownTimer.Elapsed += new ElapsedEventHandler(QueueElapsed);
                    CountdownTimer.AutoReset = true;
                    CountdownTimer.Start();
                    Client.FocusClient();
                }));
            }
            else if (Response.MethodName == "groupUpdatedV3")
            {
                // pre-match queue team lobby, welcome to the hell of LCDS calls ^^
                // This SHOULD be called only once per matchmade group, but in case its not...
                System.Diagnostics.Debug.WriteLine("groupUpdatedV3 was called");

                CallWithArgs(Guid.NewGuid().ToString(), "cap", "pickSkinV2", "{\"skinId\":" + skinId + ",\"isNewlyPurchasedSkin\":false}");
                GroupUpdate response = JsonConvert.DeserializeObject<GroupUpdate>(Response.Payload);
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    updateGroup(response);
                }));
            }
            else if (Response.MethodName == "slotPopulatedV1")
            {
                // New player joined lobby, this is fired AFTER both sides accept the invitation
                // => TODO: show players trying to join lobby (candidateFoundV2)

                // PlayerSlot will do, but this call does NOT use advertisedRole and status properties!
                PlayerSlot response = JsonConvert.DeserializeObject<PlayerSlot>(Response.Payload);
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    populateSlot(response);
                }));
            }
            else if (Response.MethodName == "soloSearchedForAnotherGroupV2")
            {
                // Someone left matchmade lobby
                SoloSearchedForAnotherGroupResponse response = JsonConvert.DeserializeObject<SoloSearchedForAnotherGroupResponse>(Response.Payload);
                if (response.slotId == teambuilderSlotId)
                {
                    //we left the team
                    // TODO: get back to queue
                    System.Diagnostics.Debug.WriteLine("we left/got kicked from team.");
                }
                else
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        PlayerListView.Items.RemoveAt(response.slotId);
                    }));
                }
                // for now, need2know what reasons are there
                if (response.reason != "SOLO_INITIATED" && response.reason != "GROUP_DISBANDED")
                    System.Diagnostics.Debug.WriteLine("soloSearchedForAnotherGroupV2 - reason was not SOLO_INITIATED! : " + response.reason);
            }
            else if (Response.MethodName == "readinessIndicatedV1")
            {
                ReadinesIndicator response = JsonConvert.DeserializeObject<ReadinesIndicator>(Response.Payload);
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    if (PlayerListView.Items.GetItemAt(response.slotId) is TeamBuilderChoose)
                    {
                        TeamBuilderChoose tbc = PlayerListView.Items.GetItemAt(response.slotId) as TeamBuilderChoose;
                        if (response.ready)
                            tbc.PlayerReadyStatus.Visibility = Visibility.Visible;
                        else
                            tbc.PlayerReadyStatus.Visibility = Visibility.Hidden;
                    }
                    else if (PlayerListView.Items.GetItemAt(response.slotId) is TeamBuilderPlayer)
                    {
                        TeamBuilderPlayer tbc = PlayerListView.Items.GetItemAt(response.slotId) as TeamBuilderPlayer;
                        if (response.ready)
                            tbc.PlayerReadyStatus.Visibility = Visibility.Visible;
                        else
                            tbc.PlayerReadyStatus.Visibility = Visibility.Hidden;
                    }
                }));
            }
            else if (Response.MethodName == "matchmakingPhaseStartedV1")
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ReadyButton.IsEnabled = false;
                    ReadyButton.Content = "Searching for match";
                }));
                inQueueTimer = 0;

                timer = new System.Timers.Timer(1000);
                timer.Elapsed += new ElapsedEventHandler((object sender, ElapsedEventArgs e) =>
                {
                    inQueueTimer++;
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        TimeSpan ts = TimeSpan.FromSeconds(inQueueTimer);
                        ReadyButton.Content = String.Format("Searching for match {0}:{1}", ts.Minutes, ts.Seconds < 10 ? "0" + ts.Seconds : "" + ts.Seconds);
                    }));
                });
                timer.AutoReset = true;
                timer.Start();
            }
            else if (Response.MethodName == "matchMadeV1")
            {
                timer.Stop();
            }
            else if (Response.MethodName == "removedFromServiceV1")
            {
                // TODO: how about we don't quit teambuilder page each time this thing is called?
                // QuitReason response = JsonConvert.DeserializeObject<QuitReason>(Response.Payload);
                // if (response.reason == "CANDIDATE_DECLINED_GROUP") else if (response.reason == "QUIT")
                System.Diagnostics.Debug.WriteLine("removed from service; no longer listening to calls");

                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    if (Client.LastPageContent == this.Content) Client.LastPageContent = null;
                    if (Client.CurrentPage == this) { Client.CurrentPage = null; Client.ReturnButton.Visibility = Visibility.Hidden; }
                }));

                Client.PVPNet.OnMessageReceived -= PVPNet_OnMessageReceived;
                Client.GameStatus = "outOfGame";
                Client.SetChatHover();
                Client.PVPNet.Leave();

                //temp, what other reasons are there?
                QuitReason response = JsonConvert.DeserializeObject<QuitReason>(Response.Payload);
                if (response.reason != "CANDIDATE_DECLINED_GROUP" && response.reason != "QUIT")
                    System.Diagnostics.Debug.WriteLine("removedFromServiceV1 - new reason! : " + response.reason);
            }
            else if (Response.MethodName.StartsWith("callFailed"))
            {
                System.Diagnostics.Debug.WriteLine("TeamBuilder error: " + Response.Status);
                System.Diagnostics.Debug.WriteLine("TeamBuilder payload: " + Response.Payload);
            }
        }

        private void populateSlot(PlayerSlot slot)
        {
            if (slot.championId == 0)
                return;
            //TODO: move PlayerName label to some not so retarded place
            if (slot.summonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
            {
                TeamBuilderChoose tbc = new TeamBuilderChoose();
                tbc.PlayerName.Content = slot.summonerName;
                tbc.PlayerName.Visibility = Visibility.Visible;
                string uriSource = System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(slot.championId).iconPath);
                tbc.Champion.Source = Client.GetImage(uriSource);
                tbc.Role.Items.Add(new Item(slot.role));
                tbc.Role.SelectedIndex = 0;
                tbc.Role.IsEnabled = false;
                tbc.Position.Items.Add(new Item(slot.position));
                tbc.Position.SelectedIndex = 0;
                tbc.Position.IsEnabled = false;
                tbc.Position.Visibility = Visibility.Visible;
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(slot.spell1Id));
                tbc.SummonerSpell1Image.Source = Client.GetImage(uriSource);
                tbc.SummonerSpell1.Visibility = Visibility.Visible;
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(slot.spell2Id));
                tbc.SummonerSpell2Image.Source = Client.GetImage(uriSource);
                tbc.SummonerSpell2.Visibility = Visibility.Visible;
                if (slot.slotId == teambuilderSlotId)
                {
                    tbc.MasteryPage.Visibility = Visibility.Visible;
                    tbc.MasteryPage.SelectionChanged += MasteryPage_SelectionChanged;
                    tbc.RunePage.Visibility = Visibility.Visible;
                    tbc.RunePage.SelectionChanged += RunePage_SelectionChanged;
                    tbc.EditMasteries.Click += EditMasteriesButton_Click;
                    tbc.EditRunes.Click += EditRunesButton_Click;
                    tbc.SummonerSpell1.Click += SummonerSpell_Click;
                    tbc.SummonerSpell2.Click += SummonerSpell_Click;

                    int i = 0;
                    foreach (MasteryBookPageDTO MasteryPage in MyMasteries.BookPages)
                    {
                        string MasteryPageName = MasteryPage.Name;
                        //Stop garbage mastery names
                        if (MasteryPageName.StartsWith("@@"))
                        {
                            MasteryPageName = "Mastery Page " + ++i;
                        }
                        tbc.MasteryPage.Items.Add(MasteryPageName);
                        if (MasteryPage.Current)
                            tbc.MasteryPage.SelectedValue = MasteryPageName;
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
                        tbc.RunePage.Items.Add(RunePageName);
                        if (RunePage.Current)
                            tbc.RunePage.SelectedValue = RunePageName;
                    }
                    TeamPlayer = tbc;
                }
                PlayerListView.Items.Insert(slot.slotId, tbc);
            }
            else
            {
                TeamBuilderPlayer tbc = new TeamBuilderPlayer();
                tbc.Height = 50;
                tbc.PlayerName.Content = slot.summonerName;
                string uriSource = System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(slot.championId).iconPath);
                tbc.Champion.Source = Client.GetImage(uriSource);
                tbc.Role.Content = slot.role;
                tbc.Position.Content = slot.position;
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(slot.spell1Id));
                tbc.SummonerSpell1Image.Source = Client.GetImage(uriSource);
                uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName(slot.spell2Id));
                tbc.SummonerSpell2Image.Source = Client.GetImage(uriSource);
                PlayerListView.Items.Insert(slot.slotId, tbc);
            }
            //just for now, need2know what other statuses are there
            if (!String.IsNullOrEmpty(slot.status) && !String.IsNullOrWhiteSpace(slot.status) && slot.status != "POPULATED" && slot.status != "CANDIDATE_FOUND")
                System.Diagnostics.Debug.WriteLine("groupUpdatedV3 - new status found! : " + slot.status);
        }

        private void ReadyButton_Click(object sender, RoutedEventArgs e)
        {
            Button readyButton = sender as Button;
            if (readyButton.Content == "Ready")
            {
                TeamPlayer.EditMasteries.IsEnabled = false;
                TeamPlayer.EditRunes.IsEnabled = false;
                TeamPlayer.SummonerSpell1.IsEnabled = false;
                TeamPlayer.SummonerSpell2.IsEnabled = false;
                TeamPlayer.MasteryPage.IsEnabled = false;
                TeamPlayer.RunePage.IsEnabled = false;
                TeamPlayer.PlayerReadyStatus.Visibility = Visibility.Visible;
                readyButton.Content = "Not Ready";
                CallWithArgs(Guid.NewGuid().ToString(), "cap", "indicateReadinessV1", "{\"ready\":true}");
            }
            else
            {
                TeamPlayer.EditMasteries.IsEnabled = true;
                TeamPlayer.EditRunes.IsEnabled = true;
                TeamPlayer.SummonerSpell1.IsEnabled = true;
                TeamPlayer.SummonerSpell2.IsEnabled = true;
                TeamPlayer.MasteryPage.IsEnabled = true;
                TeamPlayer.RunePage.IsEnabled = true;
                TeamPlayer.PlayerReadyStatus.Visibility = Visibility.Hidden;
                readyButton.Content = "Ready";
                CallWithArgs(Guid.NewGuid().ToString(), "cap", "indicateReadinessV1", "{\"ready\":false}");
            }
        }

        private void updateGroup(GroupUpdate response)
        {
            PlayerListView.Items.Clear();
            //lets just pretend this never happend mkay?
            Invite.Visibility = Visibility.Hidden;
            InvitedPlayers.Visibility = Visibility.Hidden;
            QueueButton.Visibility = Visibility.Hidden;
            ReadyButton.Visibility = Visibility.Visible;
            teambuilderSlotId = response.slotId;
            teambuilderGroupId = response.groupId;
            //TODO: find how matched team lobby chatroom name is generated
            //if(connectedToChat)
            //  LeaveChat();
            //ConnectToChat();

            foreach (PlayerSlot slot in response.slots)
                populateSlot(slot);
        }

        private void QueueElapsed(object sender, ElapsedEventArgs e)
        {
            if (TimeLeft > 0)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Timer10.Text = "" + TimeLeft;
                    TimeLeft--;
                }));
                return;
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                CountdownTimer.Stop();
                MatchFoundGrid.Visibility = Visibility.Hidden;
                CallWithArgs(Guid.NewGuid().ToString(), "cap", "quitV2", "{}");
                Client.ClearPage(typeof(TeamBuilderPage));
                Client.SwitchPage(new MainPage());
            }));
        }

        //This is something you don't know exists
        private void RenderLegenadryClientPlayerSumSpellIcons()
        {
            if (ChampionId != 0)
            {
                TeamPlayer.Champion.Source = champions.GetChampion(ChampionId).icon;
            }
            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)spell1));
            TeamPlayer.SummonerSpell1Image.Source = Client.GetImage(uriSource);
            uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "spell", SummonerSpell.GetSpellImageName((int)spell2));
            TeamPlayer.SummonerSpell2Image.Source = Client.GetImage(uriSource);
        }


        TeamBuilderChoose TeamPlayer = new TeamBuilderChoose();
        private void AddPlayer(bool inNotInTeam = true)
        {
            ///WHY ARE THERE SO MANY ROLES NOW
            TeamPlayer.PlayerName.Content = Client.LoginPacket.AllSummonerData.Summoner.Name;
            TeamPlayer.Role.Items.Add(new Item("Mage"));
            TeamPlayer.Role.Items.Add(new Item("Support"));
            TeamPlayer.Role.Items.Add(new Item("Assassin"));
            TeamPlayer.Role.Items.Add(new Item("Marksman"));
            TeamPlayer.Role.Items.Add(new Item("Fighter"));
            TeamPlayer.Role.Items.Add(new Item("Tank"));

            TeamPlayer.Position.Items.Add(new Item("Top"));
            TeamPlayer.Position.Items.Add(new Item("Middle"));
            TeamPlayer.Position.Items.Add(new Item("Bottom"));
            TeamPlayer.Position.Items.Add(new Item("Jungle"));


            //So many calls. We have to use this instead of putting the control right in because we need to put the player into the correct possition
            TeamPlayer.EditMasteries.Click += EditMasteriesButton_Click;
            TeamPlayer.EditRunes.Click += EditRunesButton_Click;

            TeamPlayer.SummonerSpell1.Click += SummonerSpell_Click;
            TeamPlayer.SummonerSpell2.Click += SummonerSpell_Click;

            TeamPlayer.Role.SelectionChanged += TeamRole_SelectionChanged;
            TeamPlayer.Position.SelectionChanged += TeamPosition_SelectionChanged;
            TeamPlayer.RunePage.SelectionChanged += RunePage_SelectionChanged;
            TeamPlayer.MasteryPage.SelectionChanged += MasteryPage_SelectionChanged;

            TeamPlayer.SelectChampion.MouseDown += Champion_MouseDown;


            int i = 0;
            foreach (MasteryBookPageDTO MasteryPage in MyMasteries.BookPages)
            {
                string MasteryPageName = MasteryPage.Name;
                //Stop garbage mastery names
                if (MasteryPageName.StartsWith("@@"))
                {
                    MasteryPageName = "Mastery Page " + ++i;
                }
                TeamPlayer.MasteryPage.Items.Add(MasteryPageName);
                if (MasteryPage.Current)
                    TeamPlayer.MasteryPage.SelectedValue = MasteryPageName;
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
                TeamPlayer.RunePage.Items.Add(RunePageName);
                if (RunePage.Current)
                    TeamPlayer.RunePage.SelectedValue = RunePageName;
            }

            PlayerListView.Items.Add(TeamPlayer);
        }


        private void Champion_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChampionsTab.IsSelected = true;
            ChampAndSkinGrid.Visibility = Visibility.Visible;
        }

        private void LockIn_Click(object sender, RoutedEventArgs e)
        {
            string uriSource = System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion(ChampionId).iconPath);
            TeamPlayer.Champion.Source = Client.GetImage(uriSource);
            ChampAndSkinGrid.Visibility = Visibility.Hidden;
        }

        private void SelectSkin(int skin)
        {
            skinId = skin;
        }
        private void SkinSelectListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
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
                        SelectSkin(0);
                        TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                        tr.Text = "Selected Default " + Champion.name + " as skin" + Environment.NewLine;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    }
                    else
                    {
                        championSkins skin = championSkins.GetSkin((int)item.Tag);
                        SelectSkin(skin.id);
                        TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                        tr.Text = "Selected " + skin.displayName + " as skin" + Environment.NewLine;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    }
                }
            }
        }

        private void SummonerSpell_Click(object sender, RoutedEventArgs e)
        {
            SelectSummonerSpells("");
            SpellsGrid.Visibility = Visibility.Visible;
        }

        private void AcceptGame_Click(object sender, RoutedEventArgs e)
        {
            CountdownTimer.Stop();
            //Super Op Code
            string LastArg = string.Format("\"acceptance\":{0},\"slotId\":{1},\"groupId\":\"{2}\"", true, teambuilderSlotId, teambuilderGroupId);
            CallWithArgs(Guid.NewGuid().ToString(), "cap", "indicateGroupAcceptanceAsCandidateV1", "{" + LastArg + "}");
            MatchFoundGrid.Visibility = Visibility.Hidden;
        }

        private void DeclineGame_Click(object sender, RoutedEventArgs e)
        {
            CountdownTimer.Stop();
            //Super Op Code
            string LastArg = string.Format("\"acceptance\":{0},\"slotId\":{1},\"groupId\":\"{2}\"", false, teambuilderSlotId, teambuilderGroupId);
            CallWithArgs(Guid.NewGuid().ToString(), "cap", "indicateGroupAcceptanceAsCandidateV1", "{" + LastArg + "}");
            Client.ClearPage(typeof(TeamBuilderPage));
            Client.SwitchPage(new MainPage());
        }

        private void TeamPosition_SelectionChanged(object sender, EventArgs e)
        {
            Item itm = (Item)TeamPlayer.Position.SelectedItem;
            position = itm.ComboRole;
            TeamPlayer.RunePage.Visibility = Visibility.Visible;
            TeamPlayer.MasteryPage.Visibility = Visibility.Visible;
            TeamPlayer.SummonerSpell1.Visibility = Visibility.Visible;
            TeamPlayer.SummonerSpell2.Visibility = Visibility.Visible;

            SelectedAllChamps();
        }

        bool QuickLoad = true;

        private async void MasteryPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                if (MasteryPageName == (string)TeamPlayer.MasteryPage.SelectedItem)
                {
                    MasteryPage.Current = true;
                    HasChanged = true;
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = "Selected " + MasteryPageName + " as Mastery Page" + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }
                bookDTO.BookPages.Add(MasteryPage);
            }
            if (HasChanged)
            {
                await Client.PVPNet.SaveMasteryBook(bookDTO);
            }
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

        private async void RunePage_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                if (RunePageName == (string)TeamPlayer.RunePage.SelectedItem)
                {
                    RunePage.Current = true;
                    SelectedRunePage = RunePage;
                    HasChanged = true;
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = "Selected " + RunePageName + " as Rune Page" + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }
            }
            if (HasChanged)
            {
                await Client.PVPNet.SelectDefaultSpellBookPage(SelectedRunePage);
            }
        }

        private void TeamRole_SelectionChanged(object sender, EventArgs e)
        {
            Item itm = (Item)TeamPlayer.Role.SelectedItem;
            role = itm.ComboRole;
            TeamPlayer.Position.Visibility = Visibility.Visible;
        }

        private void SelectedAllChamps()
        {
            //We only want this to be called when selected champs and role and position have a set value
            if (role != null && position != null && ChampionId != 0)
            {
                string roleUp = string.Format(role.ToUpper());
                string posUp = string.Format(position.ToUpper());
                string Json = string.Format("\"championId\":{0},\"position\":\"{1}\",\"role\":\"{2}\",\"queueId\":61", ChampionId, posUp, roleUp);
                CallWithArgs(Guid.NewGuid().ToString(), "cap", "retrieveEstimatedWaitTimeV2", "{" + Json + "}");
            }
        }

        private void LoadStats()
        {
            ChampionSelectListView.Items.Clear();
            if (true)
            {
                ChampList = new List<ChampionDTO>(Client.PlayerChampions);
                foreach (ChampionDTO champ in ChampList)
                {
                    champions getChamp = champions.GetChampion(champ.ChampionId);
                    if ((champ.Owned || champ.FreeToPlay))
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
        }

        private void SelectChamp(int Championid)
        {
            ChampionId = Championid;

            SkinSelectListView.Items.Clear();
            ListViewItem item = new ListViewItem();
            Image skinImage = new Image();
            ChampList = new List<ChampionDTO>(Client.PlayerChampions);
            champions Champion = champions.GetChampion(ChampionId);

            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Champion.portraitPath);

            skinImage.Source = Client.GetImage(uriSource);
            skinImage.Width = 191;
            skinImage.Stretch = Stretch.UniformToFill;
            item.Tag = "0:" + Champion.id;
            item.Content = skinImage;
            SkinSelectListView.Items.Add(item);

            foreach (ChampionDTO champ in ChampList)
            {
                if (champ.ChampionId == ChampionId)
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
            if (!LockInButton.IsEnabled)
                LockInButton.IsEnabled = true;
        }

        private void ListViewItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            SelectChamp((int)item.Tag);

            DoubleAnimation fadingAnimation = new DoubleAnimation();
            fadingAnimation.From = 1;
            fadingAnimation.To = 0;
            fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2));
            fadingAnimation.Completed += (eSender, eArgs) =>
            {
                string uriSource = System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champions.GetChampion((int)item.Tag).splashPath);
                ChampAndSkinBackgroundImage.Source = Client.GetImage(uriSource);
                fadingAnimation = new DoubleAnimation();
                fadingAnimation.From = 0;
                fadingAnimation.To = 1;
                fadingAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));

                ChampAndSkinBackgroundImage.BeginAnimation(Image.OpacityProperty, fadingAnimation);
            };
            ChampAndSkinBackgroundImage.BeginAnimation(Image.OpacityProperty, fadingAnimation);
        }

        public async void CallWithArgs(String UUID, String GameMode, String ProcedureCall, String Parameters)
        {
            await Client.PVPNet.Call(UUID, GameMode, ProcedureCall, Parameters);
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            }));
        }

        private void newRoom_OnRoomMessage(object sender, Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {

                if (msg.Body != "This room is not anonymous")
                {
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.From.Resource + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    if (Client.Filter)
                        tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "").Filter() + Environment.NewLine;
                    else
                        tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }
            }));
        }


        /// <summary>
        /// Chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (connectedToChat == true)
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (Client.Filter)
                    tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
                else
                    tr.Text = ChatTextBox.Text + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                newRoom.PublicMessage(ChatTextBox.Text);
                ChatTextBox.Text = "";
            }
            else if (connectedToChat == false)
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = "You are not connected to chat! Join a teambuilder lobby to connect to chat." + Environment.NewLine;
            }
        }

        public void SelectSummonerSpells(string GameMode = "CLASSIC")
        {
            var values = Enum.GetValues(typeof(NameToImage));
            SummonerSpellListView.Items.Clear();
            foreach (NameToImage Spell in values)
            {
                if (!availableSpells.Contains((int)Spell))
                    continue;
                Image champImage = new Image();
                champImage.Height = 64;
                champImage.Width = 64;
                champImage.Margin = new Thickness(5, 5, 5, 5);
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", "Summoner" + Spell.ToString() + ".png"), UriKind.Absolute);
                champImage.Source = new BitmapImage(uriSource);
                champImage.Tag = (int)Spell;
                SummonerSpellListView.Items.Add(champImage);
            }
        }

        private int SelectedSpell1 = 0;
        //....................................................................................................................
        //Riot why. WHY CAN'T I USE THE NORMAL WAY WITH THE POPUP. WHY DO YOU HAVE TO SEND ALL THE DATA WITH QUEUEING
        //This is the smartest thing riot has done.
        private void SummonerSpellListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SummonerSpellListView.SelectedIndex != -1)
            {
                Image item = (Image)SummonerSpellListView.SelectedItem;
                int spellId = Convert.ToInt32(item.Tag);
                NameToImage spellName = (NameToImage)spellId;
                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "spell", "Summoner" + spellName + ".png"), UriKind.Absolute);
                if (SelectedSpell1 == 0)
                {
                    SummonerSpell1.Source = new BitmapImage(uriSource);
                    SummonerSpellListView.Items.Remove(item);
                    SelectedSpell1 = spellId;
                }
                else
                {
                    SummonerSpell2.Source = new BitmapImage(uriSource);
                    SelectSpells(SelectedSpell1, spellId);
                    SelectedSpell1 = 0;
                    SpellsGrid.Visibility = Visibility.Hidden;
                }
                RenderLegenadryClientPlayerSumSpellIcons();
            }
        }

        private void SelectSpells(int Spell1, int Spell2)
        {
            spell1 = Spell1;
            spell2 = Spell2;
        }

        private async void InGame()
        {
            await Client.PVPNet.Leave();
            Client.PVPNet.OnMessageReceived -= PVPNet_OnMessageReceived;
            Client.ClearPage(typeof(TeamBuilderPage));
            Client.GameStatus = "inGame";
            Client.timeStampSince = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalMilliseconds;
            Client.SetChatHover();

            Client.SwitchPage(new InGame());
        }

        private class Item
        {
            public string ComboRole { get; set; }
            public Item(string Strings)
            {
                this.ComboRole = Strings;
            }
            public override string ToString()
            {
                return ComboRole;
            }
        }

        #region response classes
        private class ReadinesIndicator
        {
            public int slotId { get; set; }
            public bool ready { get; set; }
        }

        private class SoloSearchedForAnotherGroupResponse
        {
            public int slotId { get; set; }
            public string reason { get; set; }
            public int penaltyInSeconds { get; set; }
        }

        private class GroupUpdate
        {
            public string groupId { get; set; }
            public PlayerSlot[] slots { get; set; }
            public int slotId { get; set; }
            public int groupTtlSecs { get; set; }
        }

        private class PlayerSlot
        {
            public int slotId { get; set; }
            public string summonerName { get; set; }
            public int summonerIconId { get; set; }
            public int championId { get; set; }
            public string role { get; set; }
            public string advertisedRole { get; set; }
            public string position { get; set; }
            public int spell1Id { get; set; }
            public int spell2Id { get; set; }
            /// <summary>
            /// POPULATED, CANDIDATE_FOUND, more TBA
            /// </summary>
            public string status { get; set; }
        }

        private class InfoRetrieved
        {
            public int[] championIds { get; set; }
            public int[] skinIds { get; set; }
            public int[] spellIds { get; set; }
            public int[] initialSpellIds { get; set; }
            public bool cprAdjustmentEnabled { get; set; }
            public int[] adjustedChampionIds { get; set; }
            public string[] adjustedRoles { get; set; }
            public string[] adjustedPositions { get; set; }
            public DemandInfo demandInfo { get; set; }
        }
        private class DemandInfo
        {
            public bool areCaptainsStarved { get; set; }
            /// <summary>
            /// "role": "SUPPORT", "position": "BOTTOM", "boost": 0
            /// </summary>
            public Dictionary<String, Object>[] solosInDemand { get; set; }
        }
        private class ReceivedGroupId
        {
            public string groupId { get; set; }
            public int slotId { get; set; }
            public int candidateAutoQuitTimeout { get; set; }
        }
        private class ReceivedWaitTime
        {
            public int championId { get; set; }
            public string role { get; set; }
            public string position { get; set; }
            public int estimatedWaitTime { get; set; }
        }

        private class QuitReason
        {
            public string reason { get; set; }
        }
        #endregion

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            CallWithArgs(Guid.NewGuid().ToString(), "cap", "quitV2", "{}");
            Client.ClearPage(typeof(TeamBuilderPage));
            Client.SwitchPage(new MainPage());
        }

        private void InviteButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new InvitePlayersPage().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }
    }
}
