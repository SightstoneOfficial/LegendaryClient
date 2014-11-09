using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using jabber.protocol.client;
using LegendaryClient.Logic;
using System.Windows.Threading;
using System.Threading;
using LegendaryClient.Controls;
using System.Xml;
using System.IO;
using jabber.connection;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using System.Collections;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Game;
using System.Timers;
using System.Net;
using System.Net.NetworkInformation;
using Timer = System.Timers.Timer;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Member;
using Newtonsoft.Json;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System.Globalization;
using PVPNetConnect.RiotObjects.Platform.Game.Message;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for TeamQueuePage.xaml
    /// </summary>
    public partial class TeamQueuePage : Page
    {
        //long InviteId = 0;
        private Room newRoom;
        bool IsOwner = false;
        private Button LastSender;
        private int i = 0;
        private static Timer PingTimer;

        //gamemetadata
        int queueId, mapId, gameTypeConfigId;
        bool isRanked;
        string rankedTeamName, gameMode, gameType;

        string Invite;

        internal static LobbyStatus CurrentLobby;

        /// <summary>
        /// When invited to a team
        /// </summary>
        /// <param name="Message"></param>
        public TeamQueuePage(string Invid, LobbyStatus NewLobby = null, bool IsReturningToLobby = false)
        {
            InitializeComponent();
            Client.InviteListView = InviteListView;
            Client.PVPNet.OnMessageReceived += Update_OnMessageReceived;
            //MainWindow Window = new MainWindow();
            //Window.Hide();
            //Opps
            Invite = Invid;
            CurrentLobby = NewLobby;
            if (!IsReturningToLobby)
            {
                LoadStats();
            }

            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Lobby";
        }

        public async void LoadStats()
        {
            i = 10;
            PingTimer = new Timer(1000);
            PingTimer.Elapsed += new ElapsedEventHandler(PingElapsed);
            PingTimer.Enabled = true;
            PingElapsed(1, null);
            InviteButton.IsEnabled = false;
            StartGameButton.IsEnabled = false;

            if (CurrentLobby == null)
            {
                //Yay fixed lobby. Riot hates me still though. They broke this earlier, except I have fixed it
                CurrentLobby = await Client.PVPNet.getLobbyStatus();
            }
            if (CurrentLobby.InvitationID != null)
            {
                string ObfuscatedName = Client.GetObfuscatedChatroomName(CurrentLobby.InvitationID.Replace("INVID", "invid"), ChatPrefixes.Arranging_Game); //Why do you need to replace INVID with invid Riot?
                string JID = Client.GetChatroomJID(ObfuscatedName, CurrentLobby.ChatKey, false);
                newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
                newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
                newRoom.OnRoomMessage += newRoom_OnRoomMessage;
                newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
                newRoom.Join(CurrentLobby.ChatKey);

                RenderLobbyData();
            }
            else
            {
                Client.ClearPage(typeof(TeamQueuePage));
                Client.SwitchPage(new MainPage());
                Client.Log("Failed to join room.");
            }
        }


        private async void Inviter_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            Member stats = (Member)LastSender.Tag;
            await Client.PVPNet.GrantInvite(stats.SummonerId);

        }
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            Member stats = (Member)LastSender.Tag;
            Client.SwitchPage(new ProfilePage(stats.SummonerName));
        }
        private async void Kick_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            Member stats = (Member)LastSender.Tag;
            await Client.PVPNet.Kick(stats.SummonerId);
        }
        private async void Owner_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            Member stats = (Member)LastSender.Tag;
            await Client.PVPNet.MakeOwner(stats.SummonerId);
        }

        internal void PingElapsed(object sender, ElapsedEventArgs e)
        {
            if (i++ < 10)//Ping every 10 seconds
                return;
            i = 0;
            double PingAverage = HighestPingTime(Client.Region.PingAddresses);
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                //Ping
                PingLabel.Text = Math.Round(PingAverage).ToString() + "ms";
                if (PingAverage == 0)
                {
                    PingLabel.Text = "Timeout";
                }
                if (PingAverage == -1)
                {
                    PingLabel.Text = "Ping not enabled for this region";
                }
                BrushConverter bc = new BrushConverter();
                Brush brush = (Brush)bc.ConvertFrom("#FFFF6767");
                if (PingAverage > 999 || PingAverage < 1)
                {
                    PingRectangle.Fill = brush;
                }
                brush = (Brush)bc.ConvertFrom("#FFFFD667");
                if (PingAverage > 110 && PingAverage < 999)
                {
                    PingRectangle.Fill = brush;
                }
                brush = (Brush)bc.ConvertFrom("#FF67FF67");
                if (PingAverage < 110 && PingAverage > 1)
                {
                    PingRectangle.Fill = brush;
                }
            }));
        }
        internal double HighestPingTime(IPAddress[] Addresses)
        {
            double HighestPing = -1;
            if (Addresses.Length > 0)
            {
                HighestPing = 0;
            }
            foreach (IPAddress Address in Addresses)
            {
                int timeout = 120;
                Ping pingSender = new Ping();
                PingReply reply = pingSender.Send(Address.ToString(), timeout);
                if (reply.Status == IPStatus.Success)
                {
                    if (reply.RoundtripTime > HighestPing)
                    {
                        HighestPing = reply.RoundtripTime;
                    }
                }
            }
            return HighestPing;
        }

        internal TeamControl TeamPlayerStats;

        private void RenderLobbyData()
        {
            try 
            {
                int Players = 0;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async() =>
                {
                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    TextInfo textInfo = cultureInfo.TextInfo;

                    Client.InviteListView.Items.Clear();
                    TeamListView.Items.Clear();
                    IsOwner = false;

                    if(CurrentLobby.Owner.SummonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
                    {
                        IsOwner = true;
                    }

                    foreach (Invitee statsx in CurrentLobby.Invitees)
                    {
                        var InviteeState = string.Format(statsx.inviteeState.ToLower());
                        var InviteeStateTitleCase = textInfo.ToTitleCase(InviteeState);
                        InvitePlayer invitePlayer = new InvitePlayer();
                        invitePlayer.StatusLabel.Content = InviteeStateTitleCase;
                        invitePlayer.PlayerLabel.Content = statsx.SummonerName;
                        Client.InviteListView.Items.Add(invitePlayer);
                    }

                    if (IsOwner == true)
                    {
                        InviteButton.IsEnabled = true;
                        StartGameButton.IsEnabled = true;
                        Client.isOwnerOfGame = true;
                    }
                    else if (IsOwner == false)
                    {
                        InviteButton.IsEnabled = false;
                        StartGameButton.IsEnabled = false;
                        Client.isOwnerOfGame = false;
                    }
                    invitationRequest m = JsonConvert.DeserializeObject<invitationRequest>(CurrentLobby.GameData);
                    queueId = m.queueId;
                    isRanked = m.isRanked;
                    rankedTeamName = m.rankedTeamName;
                    mapId = m.mapId;
                    gameTypeConfigId = m.gameTypeConfigId;
                    gameMode = m.gameMode;
                    gameType = m.gameType;

                    foreach (Member stats in CurrentLobby.Members)
                    {
                    //Your kidding me right
                        TeamControl TeamPlayer = new TeamControl();
                        TeamPlayerStats = TeamPlayer;
                        TeamPlayer.Name.Content = stats.SummonerName;
                        TeamPlayer.SumID.Content = stats.SummonerName;
                        TeamPlayer.Kick.Tag = stats;
                        TeamPlayer.Inviter.Tag = stats;
                        TeamPlayer.Profile.Tag = stats;
                        TeamPlayer.Owner.Tag = stats;
                        TeamPlayer.Width = 1500;
                        TeamPlayer.HorizontalAlignment = HorizontalAlignment.Stretch;

                        TeamPlayer.Kick.Click += Kick_Click;
                        TeamPlayer.Inviter.Click += Inviter_Click;
                        TeamPlayer.Profile.Click += Profile_Click;
                        TeamPlayer.Owner.Click += Owner_Click;
                        Players++;

                        PublicSummoner Summoner = await Client.PVPNet.GetSummonerByName(stats.SummonerName);

                        //Populate the ProfileIcon
                        int ProfileIconID = Summoner.ProfileIconId;
                        var uriSource = System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", ProfileIconID + ".png");

                        TeamPlayer.ProfileIcon.Source = Client.GetImage(uriSource);

                        //Make it so you cant kick yourself
                        if (stats.SummonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
                        {
                            TeamPlayer.Kick.Visibility = Visibility.Hidden;
                            TeamPlayer.Inviter.Visibility = Visibility.Hidden;
                            TeamPlayer.Profile.Visibility = Visibility.Hidden;
                            TeamPlayer.Owner.Visibility = Visibility.Hidden;
                            if (stats.hasDelegatedInvitePower == true && IsOwner == false)
                            {
                                InviteButton.IsEnabled = true;
                            }
                            else if (stats.hasDelegatedInvitePower == false && IsOwner == false)
                            {
                                InviteButton.IsEnabled = false;
                            }
                        }
                        if (IsOwner == false)
                        {
                            //So you don't crash trying to kick someone when you can't
                            TeamPlayer.Kick.Visibility = Visibility.Hidden;
                            TeamPlayer.Inviter.Visibility = Visibility.Hidden;
                            TeamPlayer.Owner.Visibility = Visibility.Hidden;
                        }
                        TeamListView.Items.Add(TeamPlayer);
                    }
                    if (queueId == 4)
                    {
                        if (Players >= 2)
                            InviteButton.IsEnabled = false;
                        else
                            InviteButton.IsEnabled = true;
                    }
                    
                }));
            }
            catch { }
            
        }

        private void Update_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(LobbyStatus))
            {
                LobbyStatus Lobby = message as LobbyStatus;
                CurrentLobby = Lobby;
                RenderLobbyData();
            }
            else if (message is GameDTO)
            {
                GameDTO Queue = message as GameDTO;
                if (Queue.GameState == "TERMINATED")
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        if (Queue.TerminatedCondition != "NOT_TERMINATED")
                        {
                            setStartButtonText("Start Game");
                            inQueue = false;
                            Client.PVPNet.PurgeFromQueues();
                        }
                        else
                        {
                            Client.PVPNet.OnMessageReceived += GotQueuePop;
                        }
                    }));
                }
            }
            else if (message is GameNotification)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    setStartButtonText("Start Game");
                    inQueue = false;
                }));
            }
            else if (message is SearchingForMatchNotification)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    EnteredQueue(message as SearchingForMatchNotification);
                }));
            }
        }

        public void Invite_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new InvitePlayersPage().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private async void Leave_Click(object sender, RoutedEventArgs e)
        {
            await Client.PVPNet.Leave();
            await Client.PVPNet.PurgeFromQueues();
            Client.ClearPage(typeof(TeamQueuePage));
            Client.SwitchPage(new MainPage());
            Client.ReturnButton.Visibility = Visibility.Hidden;
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatText.ScrollToEnd();
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
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    ChatText.ScrollToEnd();
                }
            }));
        }

        private void GotQueuePop(object sender, object message)
        {
            if (Client.runonce == false && message is GameDTO)
            {
                GameDTO Queue = message as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Client.OverlayContainer.Content = new QueuePopOverlay(Queue, this).Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                Client.PVPNet.OnMessageReceived -= GotQueuePop;
            }
        }

        public void readdHandler()
        {
            Client.PVPNet.OnMessageReceived += GotQueuePop;
        }

        private bool DevMode = false, makeRanked = false;

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChatTextBox.Text == "!~dev")
            {
                DevMode = !DevMode;
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = "DEV MODE: " + DevMode + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatTextBox.Text = "";
                if (DevMode) CreateRankedCheckBox.Visibility = Visibility.Visible;
                else CreateRankedCheckBox.Visibility = Visibility.Hidden;
            }
            else
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = ChatTextBox.Text + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                if (String.IsNullOrEmpty(ChatTextBox.Text))
                    return;
                newRoom.PublicMessage(ChatTextBox.Text);
                ChatTextBox.Text = "";
                ChatText.ScrollToEnd();
            }
        }

        internal List<Int32> QueueIds;

        bool inQueue = false;

#pragma warning disable 4014

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!inQueue)
            {
                MatchMakerParams parameters = new MatchMakerParams();
                parameters.Languages = null;
                QueueIds = new List<int>();
                QueueIds.Add(queueId);
                parameters.QueueIds = (makeRanked ? new int[] { 4 } : QueueIds.ToArray());
                parameters.InvitationId = CurrentLobby.InvitationID;
                parameters.TeamId = null;
                parameters.LastMaestroMessage = null;
                List<int> InviteList = new List<int>();
                foreach (Member stats in CurrentLobby.Members)
                {
                    int GameInvitePlayerList = Convert.ToInt32(stats.SummonerId);
                    InviteList.Add(GameInvitePlayerList);
                }
                parameters.Team = InviteList;

                Client.PVPNet.AttachTeamToQueue(parameters, new SearchingForMatchNotification.Callback(EnteredQueue));
            }
            else
            {
                Client.PVPNet.PurgeFromQueues();
                setStartButtonText("Start Game");
                inQueue = false;
            }
        }

        private void setStartButtonText(string text)
        {
            Dispatcher.Invoke(new Action(() => { StartGameButton.Content = text; }));
        }

        private void EnteredQueue(SearchingForMatchNotification result)
        {
            if (result.PlayerJoinFailures != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    MessageOverlay messageOver = new MessageOverlay();
                    messageOver.MessageTitle.Content = "Could not join the queue";
                    foreach (QueueDodger x in result.PlayerJoinFailures)
                    {
                        messageOver.MessageTextBox.Text += x.Summoner.Name + " is unable to join the queue as they recently dodged a game." + Environment.NewLine;
                        TimeSpan time = TimeSpan.FromMilliseconds(x.PenaltyRemainingTime);
                        messageOver.MessageTextBox.Text += "You have " + string.Format("{0:D2}m:{1:D2}s", time.Minutes, time.Seconds) + " remaining until you may queue again";
                    }
                    Client.OverlayContainer.Content = messageOver.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                return;
            }
            Client.PVPNet.OnMessageReceived += GotQueuePop;
            setStartButtonText("In Queue...");
            inQueue = true;
        }

        private void AutoAcceptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Client.AutoAcceptQueue = (AutoAcceptCheckBox.IsChecked.HasValue) ? AutoAcceptCheckBox.IsChecked.Value : false;
        }

        private void CreateRankedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            //To quadra queue ranked, simply make a Normal 5v5 game then check the box.
            //Does not bypass division differences, only allows multiple people as the lobby isn't that of ranked.
            makeRanked = !makeRanked;
        }
    }
}