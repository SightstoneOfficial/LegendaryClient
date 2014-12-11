using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using jabber;
using jabber.connection;
using jabber.protocol.client;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using Newtonsoft.Json;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Game.Message;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Member;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using PVPNetConnect.RiotObjects.Platform.ServiceProxy.Dispatch;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using Timer = System.Timers.Timer;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for TeamQueuePage.xaml
    /// </summary>
    public partial class TeamQueuePage
    {
        //long InviteId = 0;
        private Room newRoom;
        private bool IsOwner;
        private Button LastSender;
        private int i;
        private static Timer PingTimer;

        //gamemetadata
        private int queueId, mapId, gameTypeConfigId;
        private bool isRanked;
        private string rankedTeamName, gameMode, gameType;

        private string Invite;

        internal static LobbyStatus CurrentLobby;

        /// <summary>
        ///     When invited to a team
        /// </summary>
        /// <param name="Message"></param>
        public TeamQueuePage(string Invid, LobbyStatus NewLobby = null, bool IsReturningToLobby = false,
            bool isranked = false)
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

            if (isranked)
                DevMode = true;

            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Lobby";
        }

        public async void LoadStats()
        {
            i = 10;
            PingTimer = new Timer(1000);
            PingTimer.Elapsed += PingElapsed;
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
                string ObfuscatedName =
                    Client.GetObfuscatedChatroomName(CurrentLobby.InvitationID.Replace("INVID", "invid"),
                        ChatPrefixes.Arranging_Game); //Why do you need to replace INVID with invid Riot?
                string JID = Client.GetChatroomJID(ObfuscatedName, CurrentLobby.ChatKey, false);
                newRoom = Client.ConfManager.GetRoom(new JID(JID));
                newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
                newRoom.OnRoomMessage += newRoom_OnRoomMessage;
                newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
                newRoom.Join(CurrentLobby.ChatKey);

                RenderLobbyData();
            }
            else
            {
                Client.GameStatus = "outOfGame";
                Client.SetChatHover();
                Client.ClearPage(typeof (TeamQueuePage));
                Client.SwitchPage(new MainPage());
                Client.Log("Failed to join room.");
            }
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button) sender;
            var stats = (Member) LastSender.Tag;
            uiLogic.UpdateProfile(stats.SummonerName);
            Client.SwitchPage(uiLogic.Profile);
        }

        private async void Kick_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button) sender;
            var stats = (Member) LastSender.Tag;
            await Client.PVPNet.Kick(stats.SummonerId);
        }

        private async void Owner_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button) sender;
            var stats = (Member) LastSender.Tag;
            await Client.PVPNet.MakeOwner(stats.SummonerId);
        }

        private double startTime;

        internal void PingElapsed(object sender, ElapsedEventArgs e)
        {
            if (i++ < 10)
            {
                if (inQueue)
                {
                    TimeSpan time = TimeSpan.FromSeconds(startTime);
                    startTime++;
                    setStartButtonText(string.Format("{0:D2}:{1:D2} Re-Click To Leave", time.Minutes, time.Seconds));
                }
                return;
            }
            i = 0;
            double PingAverage = HighestPingTime(Client.Region.PingAddresses);
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                //Ping
                PingLabel.Text = Math.Round(PingAverage) + "ms";
                if (PingAverage == 0)
                {
                    PingLabel.Text = "Timeout";
                }
                if (PingAverage == -1)
                {
                    PingLabel.Text = "Ping not enabled for this region";
                }
                var bc = new BrushConverter();
                var brush = (Brush) bc.ConvertFrom("#FFFF6767");
                if (PingAverage > 999 || PingAverage < 1)
                {
                    PingRectangle.Fill = brush;
                }
                brush = (Brush) bc.ConvertFrom("#FFFFD667");
                if (PingAverage > 110 && PingAverage < 999)
                {
                    PingRectangle.Fill = brush;
                }
                brush = (Brush) bc.ConvertFrom("#FF67FF67");
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
                var pingSender = new Ping();
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
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    TextInfo textInfo = cultureInfo.TextInfo;

                    Client.InviteListView.Items.Clear();
                    TeamListView.Items.Clear();
                    IsOwner = false;

                    if (CurrentLobby.Owner != null &&
                        CurrentLobby.Owner.SummonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
                    {
                        IsOwner = true;
                    }

                    foreach (Invitee statsx in CurrentLobby.Invitees)
                    {
                        string InviteeState = string.Format(statsx.inviteeState.ToLower());
                        string InviteeStateTitleCase = textInfo.ToTitleCase(InviteeState);
                        var invitePlayer = new InvitePlayer();
                        invitePlayer.StatusLabel.Content = InviteeStateTitleCase;
                        invitePlayer.PlayerLabel.Content = statsx.SummonerName;
                        Client.InviteListView.Items.Add(invitePlayer);
                    }

                    if (IsOwner)
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
                    var m = JsonConvert.DeserializeObject<invitationRequest>(CurrentLobby.GameData);
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
                        var TeamPlayer = new TeamControl();
                        TeamPlayerStats = TeamPlayer;
                        TeamPlayer.Name.Content = stats.SummonerName;
                        TeamPlayer.SumId.Content = stats.SummonerName;
                        TeamPlayer.Kick.Tag = stats;
                        TeamPlayer.Inviter.Tag = stats;
                        TeamPlayer.UnInviter.Tag = stats;
                        TeamPlayer.Profile.Tag = stats;
                        TeamPlayer.Owner.Tag = stats;
                        TeamPlayer.Width = 1500;
                        TeamPlayer.HorizontalAlignment = HorizontalAlignment.Stretch;

                        TeamPlayer.Kick.Click += Kick_Click;
                        TeamPlayer.Inviter.Click += async (object sender, RoutedEventArgs e) =>
                        {
                            LastSender = (Button) sender;
                            var s = (Member) LastSender.Tag;
                            await Client.PVPNet.GrantInvite(s.SummonerId);
                            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                TeamPlayer.Inviter.Visibility = Visibility.Hidden;
                                TeamPlayer.UnInviter.Visibility = Visibility.Visible;
                            }));
                        };
                        TeamPlayer.UnInviter.Click += async (object sender, RoutedEventArgs e) =>
                        {
                            LastSender = (Button) sender;
                            var s = (Member) LastSender.Tag;
                            await Client.PVPNet.revokeInvite(s.SummonerId);
                            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                TeamPlayer.Inviter.Visibility = Visibility.Visible;
                                TeamPlayer.UnInviter.Visibility = Visibility.Hidden;
                            }));
                        };
                        TeamPlayer.Profile.Click += Profile_Click;
                        TeamPlayer.Owner.Click += Owner_Click;
                        Players++;

                        PublicSummoner Summoner = await Client.PVPNet.GetSummonerByName(stats.SummonerName);

                        //Populate the ProfileIcon
                        int ProfileIconID = Summoner.ProfileIconId;
                        string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon",
                            ProfileIconID + ".png");

                        TeamPlayer.ProfileIcon.Source = Client.GetImage(uriSource);

                        //Make it so you cant kick yourself
                        if (stats.SummonerName == Client.LoginPacket.AllSummonerData.Summoner.Name)
                        {
                            TeamPlayer.Kick.Visibility = Visibility.Hidden;
                            TeamPlayer.Inviter.Visibility = Visibility.Hidden;
                            TeamPlayer.UnInviter.Visibility = Visibility.Hidden;
                            TeamPlayer.Profile.Visibility = Visibility.Hidden;
                            TeamPlayer.Owner.Visibility = Visibility.Hidden;
                            if (stats.hasDelegatedInvitePower && IsOwner == false)
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
                            TeamPlayer.UnInviter.Visibility = Visibility.Hidden;
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
                    if (IsOwner)
                    {
                        Client.PVPNet.Call(Guid.NewGuid().ToString(), "suggestedPlayers",
                            "retrieveOnlineFriendsOfFriends", "{\"queueId\":" + queueId + "}");
                    }
                }));
            }
            catch
            {
            }
        }

        private void Update_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof (LobbyStatus))
            {
                var Lobby = message as LobbyStatus;
                CurrentLobby = Lobby;
                RenderLobbyData();
            }
            else if (message is GameDTO)
            {
                var QueueDTO = message as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    if (QueueDTO.GameState == "TERMINATED")
                    {
                        Client.OverlayContainer.Visibility = Visibility.Hidden;
                        Client.OverlayContainer.Content = null;
                        if (QueueDTO.QueuePosition == 0) //They changed this as soon as I fixed it. Damnit riot lol.
                        {
                            setStartButtonText("Start Game");
                            inQueue = false;
                            Client.PVPNet.PurgeFromQueues();
                        }
                        else
                        {
                            Client.PVPNet.OnMessageReceived += GotQueuePop;
                        }
                    }
                }));
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
                Dispatcher.BeginInvoke(DispatcherPriority.Input,
                    new ThreadStart(() => { EnteredQueue(message as SearchingForMatchNotification); }));
            }
            else if (message is InvitePrivileges)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var priv = message as InvitePrivileges;
                    if (priv.canInvite)
                    {
                        var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                        tr.Text = "You may invite players to this game." + Environment.NewLine;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                        InviteButton.IsEnabled = true;
                    }
                    else
                    {
                        var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                        tr.Text = "You may no longer invite players to this game." + Environment.NewLine;
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                        InviteButton.IsEnabled = false;
                    }
                }));
            }
            else if (message is LcdsServiceProxyResponse)
            {
                parseLcdsMessage(message as LcdsServiceProxyResponse); //Don't look there, its ugly!!! :)))
            }
        }

        private void parseLcdsMessage(LcdsServiceProxyResponse ProxyResponse)
        {
            if (ProxyResponse.MethodName == "retrieveOnlineFriendsOfFriends")
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    FriendsOfFriendsView.Items.Clear();
                    var suggestedFriends = JsonConvert.DeserializeObject<SuggestedFriend[]>(ProxyResponse.Payload);
                    foreach (SuggestedFriend s in suggestedFriends)
                    {
                        var invitePlayer = new SuggestedPlayerItem();
                        invitePlayer.PlayerLabel.Content = s.summonerName;
                        invitePlayer.InviteButton.Click += (object obj, RoutedEventArgs e) =>
                        {
                            Client.PVPNet.InviteFriendOfFriend(s.summonerId, s.commonFriendId);
                            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                foreach (SuggestedPlayerItem item in FriendsOfFriendsView.Items)
                                {
                                    if ((string) item.PlayerLabel.Content == s.summonerName)
                                    {
                                        item.InviteButton.IsEnabled = false;
                                        item.InviteButton.Content = "Invited";
                                        var t = new Timer();
                                        t.AutoReset = false;
                                        t.Elapsed += (object source, ElapsedEventArgs args) =>
                                        {
                                            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                                            {
                                                item.InviteButton.IsEnabled = true;
                                                item.InviteButton.Content = "Invite";
                                            }));
                                        };
                                        t.Interval = 5000;
                                        t.Start();
                                    }
                                }
                            }));
                        };
                        FriendsOfFriendsView.Items.Add(invitePlayer);
                    }
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
            Client.GameStatus = "outOfGame";
            Client.SetChatHover();
            Client.ClearPage(typeof (TeamQueuePage));
            Client.SwitchPage(new MainPage());
            Client.ReturnButton.Visibility = Visibility.Hidden;
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
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
                    var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.From.Resource + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    if (Client.Filter)
                        tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "").Filter() +
                                  Environment.NewLine;
                    else
                        tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    ChatText.ScrollToEnd();
                }
            }));
        }

        private void GotQueuePop(object sender, object message)
        {
            if (Client.runonce == false && message is GameDTO &&
                (message as GameDTO).GameState == "JOINING_CHAMP_SELECT")
            {
                var Queue = message as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Client.OverlayContainer.Content = new QueuePopOverlay(Queue, this).Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                Client.PVPNet.OnMessageReceived -= GotQueuePop;
            }
        }

        private bool DevMode, makeRanked;

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChatTextBox.Text == "!~dev")
            {
                DevMode = !DevMode;
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = "DEV MODE: " + DevMode + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatTextBox.Text = "";
                if (DevMode)
                {
#if DEBUG
                    CreateRankedCheckBox.Visibility = Visibility.Visible;
#endif
                    SelectChamp.Visibility = Visibility.Visible;
                }
                else
                {
                    CreateRankedCheckBox.Visibility = Visibility.Hidden;
                    SelectChamp.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (Client.Filter)
                    tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
                else
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

        private bool inQueue;

#pragma warning disable 4014

        private void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!inQueue)
            {
                var parameters = new MatchMakerParams();
                parameters.Languages = null;
                QueueIds = new List<int>();
                QueueIds.Add(queueId);
                parameters.QueueIds = (makeRanked ? new[] {4} : QueueIds.ToArray());
                parameters.InvitationId = CurrentLobby.InvitationID;
                parameters.TeamId = null;
                parameters.LastMaestroMessage = null;
                var InviteList = new List<int>();
                foreach (Member stats in CurrentLobby.Members)
                {
                    int GameInvitePlayerList = Convert.ToInt32(stats.SummonerId);
                    InviteList.Add(GameInvitePlayerList);
                }
                parameters.Team = InviteList;
                Client.PVPNet.AttachTeamToQueue(parameters, EnteredQueue);
            }
            else
            {
                Client.PVPNet.PurgeFromQueues();
                setStartButtonText("Start Game");
                inQueue = false;
                Client.GameStatus = "outOfGame";
                Client.SetChatHover();
            }
        }

        private void setStartButtonText(string text)
        {
            Dispatcher.Invoke(() => { StartGameButton.Content = text; });
        }

        private void EnteredQueue(SearchingForMatchNotification result)
        {
            if (result.PlayerJoinFailures != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var messageOver = new MessageOverlay();
                    messageOver.MessageTitle.Content = "Could not join the queue";
                    foreach (QueueDodger x in result.PlayerJoinFailures)
                    {
                        TimeSpan time = TimeSpan.FromMilliseconds(x.PenaltyRemainingTime);
                        switch (x.ReasonFailed)
                        {
                            case "LEAVER_BUSTER_TAINT_WARNING":
                                messageOver.MessageTextBox.Text +=
                                    " - You have left a game in progress. Please use the official client to remove the warning for now.";
                                //Need to implement their new warning for leaving.
                                break;
                            case "QUEUE_DODGER":
                                messageOver.MessageTextBox.Text += " - " + x.Summoner.Name +
                                                                   " is unable to join the queue as they recently dodged a game." +
                                                                   Environment.NewLine;
                                messageOver.MessageTextBox.Text += " - You have " +
                                                                   string.Format("{0:D2}m:{1:D2}s", time.Minutes,
                                                                       time.Seconds) +
                                                                   " remaining until you may queue again";
                                break;
                            case "QUEUE_RESTRICTED":
                                messageOver.MessageTextBox.Text +=
                                    " - You are too far apart in ranked to queue together.";
                                messageOver.MessageTextBox.Text +=
                                    " - For instance, Silvers can only queue with Bronze, Silver, or Gold players.";
                                break;
                            case "RANKED_RESTRICTED":
                                messageOver.MessageTextBox.Text +=
                                    " - You are not currently able to queue for ranked for: " + x.PenaltyRemainingTime +
                                    " games. If this is inaccurate please report it as an issue on the github page. Thanks!";
                                break;
                            default:
                                messageOver.MessageTextBox.Text += "Please submit: - " + x.ReasonFailed +
                                                                   " - as an Issue on github explaining what it meant. Thanks!";
                                break;
                        }
                    }
                    Client.OverlayContainer.Content = messageOver.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                return;
            }
            Client.PVPNet.OnMessageReceived += GotQueuePop;
            setStartButtonText("Joining Queue");
            startTime = 1;
            inQueue = true;
            Client.GameStatus = "inQueue";
            Client.timeStampSince =
                (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalMilliseconds;
            Client.SetChatHover();
        }

        private void AutoAcceptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Client.AutoAcceptQueue = (AutoAcceptCheckBox.IsChecked.HasValue)
                ? AutoAcceptCheckBox.IsChecked.Value
                : false;
        }

        private void CreateRankedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            makeRanked = !makeRanked;
        }

        private void SelectChamp_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new SelectChampOverlay(this).Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        internal void CreateText(string text, SolidColorBrush color)
        {
            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            tr.Text = text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }

        private class SuggestedFriend
        {
            public double summonerId { get; set; }
            public double commonFriendId { get; set; }
            public string summonerName { get; set; }
            public string commonFriendName { get; set; }
            public string SuggestedPlayerType { get; set; }
        }
    }
}