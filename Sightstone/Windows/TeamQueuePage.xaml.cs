﻿using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.SQLite;
using Sightstone.Properties;
using Newtonsoft.Json;
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
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using RtmpSharp.IO;
using Timer = System.Timers.Timer;
using Sightstone.Logic.Riot.Team;
using RtmpSharp.Messaging;
using agsXMPP;
using agsXMPP.protocol.x.muc;
using agsXMPP.protocol.client;
using agsXMPP.Collections;
using Sightstone.Logic.JSON;
using Sightstone.Logic.MultiUser;
using System.Windows.Media.Imaging;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for TeamQueuePage.xaml
    /// </summary>
    public partial class TeamQueuePage
    {
        //long InviteId = 0;
        private MucManager newRoom;
        private Jid jid;
        private bool IsOwner;
        private Button LastSender;
        private int i;
        private static Timer PingTimer;
        private TeamId selectedTeamId;
        private MatchMakerParams parameters;

        //gamemetadata
        private int queueId, mapId, gameTypeConfigId;
        private bool isRanked;
        private string rankedTeamName, gameMode, gameType, botDifficulty;

        private string Invite;
        private UserClient UserClient;

        internal static LobbyStatus CurrentLobby;

        /// <summary>
        ///     When invited to a team
        /// </summary>
        public TeamQueuePage(string Invid, LobbyStatus NewLobby = null, bool IsReturningToLobby = false, TeamId SelectedTeam = null, string BotDifficulty = null)
        {
            InitializeComponent();
            UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            Client.InviteListView = InviteListView;
            UserClient.RiotConnection.MessageReceived += Update_OnMessageReceived;

            //MainWindow Window = new MainWindow();
            //Window.Hide();
            //Opps
            Invite = Invid;
            CurrentLobby = NewLobby;
            selectedTeamId = SelectedTeam;
            botDifficulty = BotDifficulty;
            if (!IsReturningToLobby)
            {
                LoadStats();
            }

            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = Client.GetDictText("ReturnToLobby");
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
                CurrentLobby = await UserClient.calls.AcceptInvite(Invite);
            }
            if (CurrentLobby.InvitationID != null)
            {
                string ObfuscatedName =
                    Client.GetObfuscatedChatroomName(CurrentLobby.InvitationID.ToLower(),
                        ChatPrefixes.Arranging_Game);
                string Jid = Client.GetChatroomJid(ObfuscatedName, CurrentLobby.ChatKey, false);
                newRoom = new MucManager(UserClient.XmppConnection);
                jid = new Jid(Jid);
                UserClient.XmppConnection.OnMessage += XmppConnection_OnMessage;
                UserClient.XmppConnection.OnPresence += XmppConnection_OnPresence;
                newRoom.AcceptDefaultConfiguration(jid);
                newRoom.JoinRoom(jid, UserClient.LoginPacket.AllSummonerData.Summoner.Name, CurrentLobby.ChatKey);
                RenderLobbyData();
            }
            else
            {
                UserClient.GameStatus = "outOfGame";
                UserClient.SetChatHover();
                Client.SwitchPage(Client.MainPage);
                Client.ClearPage(typeof(TeamQueuePage));
                Client.Log("Failed to join room.");
            }
        }

        void XmppConnection_OnPresence(object sender, Presence pres)
        {
            if (pres.To.Bare != jid.Bare)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = Client.GetDictText("UserJoinRoomNotify").Replace("{USER}", pres.From.Resource) + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatText.ScrollToEnd();
            }));
        }

        void XmppConnection_OnMessage(object sender, Message msg)
        {
            if (jid.Bare.Contains(msg.From.User))
                return;

            if (msg.From.Resource == UserClient.LoginPacket.AllSummonerData.Summoner.Name)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (msg.Body == "This room is not anonymous") return;
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.From.Resource + ": "
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Turquoise);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (UserClient.Filter)
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "").Filter() +
                              Environment.NewLine;
                else
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                ChatText.ScrollToEnd();
            }));
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            var stats = (Member)LastSender.Tag;
            Client.Profile.GetSummonerProfile(stats.SummonerName);
            Client.SwitchPage(Client.Profile);
        }

        private async void Kick_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            var stats = (Member)LastSender.Tag;
            await UserClient.calls.Kick(stats.SummonerId);
        }

        private async void Owner_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            var stats = (Member)LastSender.Tag;
            await UserClient.calls.MakeOwner(stats.SummonerId);
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

                    Dispatcher.Invoke(() =>
                    {
                        Client.inQueueTimer.Content =
                            $"{Client.GetDictText("InQueue")} {time.Minutes:D2}:{time.Seconds:D2}";
                    });
                    setStartButtonText(Client.GetDictText("ReClickLeave"));
                }
                return;
            }
            i = 0;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                //Ping
                var bc = new BrushConverter();
                Brush brush = null;
                try
                {
                    double pingAverage = HighestPingTime(UserClient.Region.PingAddresses);
                    PingLabel.Text = Math.Round(pingAverage) + "ms";
                    if (pingAverage == 0)
                        PingLabel.Text = Client.GetDictText("Timeout");

                    if (pingAverage == -1)
                        PingLabel.Text = Client.GetDictText("PingNotEnabled");

                    if (pingAverage > 999 || pingAverage < 1)
                        brush = (Brush)bc.ConvertFrom("#FFFF6767");

                    if (pingAverage > 110 && pingAverage < 999)
                        brush = (Brush)bc.ConvertFrom("#FFFFD667");

                    if (pingAverage < 110 && pingAverage > 1)
                        brush = (Brush)bc.ConvertFrom("#FF67FF67");

                }
                catch (NotImplementedException ex)
                {
                    PingLabel.Text = Client.GetDictText("PingNotEnabled");
                    brush = (Brush)bc.ConvertFrom("#FFFF6767");
                    Client.Log(ex.Message);
                }
                catch (Exception ex)
                {
                    PingLabel.Text = Client.GetDictText("PingError");
                    brush = (Brush)bc.ConvertFrom("#FFFF6767");
                    Client.Log(ex.Message);
                }
                finally
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
                        CurrentLobby.Owner.SummonerName == UserClient.LoginPacket.AllSummonerData.Summoner.Name)
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
                        switch (InviteeState)
                        {
                            case "owner":
                            case "accepted":
                            case "creator":
                                invitePlayer.StatusLabel.Foreground = Brushes.Green;
                                break;
                            case "pending":
                                invitePlayer.StatusLabel.Foreground = Brushes.Yellow;
                                break;
                            case "declined":
                            case "quit":
                                invitePlayer.StatusLabel.Foreground = Brushes.Red;
                                break;
                        }
                        Client.InviteListView.Items.Add(invitePlayer);
                    }

                    if (IsOwner)
                    {
                        InviteButton.IsEnabled = true;
                        StartGameButton.IsEnabled = true;
                        UserClient.isOwnerOfGame = true;
                    }
                    else if (IsOwner == false)
                    {
                        InviteButton.IsEnabled = false;
                        StartGameButton.IsEnabled = false;
                        UserClient.isOwnerOfGame = false;
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
                        TeamPlayer.SummonerName.Content = stats.SummonerName;
                        TeamPlayer.SumId.Content = stats.SummonerName;
                        TeamPlayer.Kick.Tag = stats;
                        TeamPlayer.Inviter.Tag = stats;
                        TeamPlayer.UnInviter.Tag = stats;
                        TeamPlayer.Profile.Tag = stats;
                        TeamPlayer.Owner.Tag = stats;
                        TeamPlayer.Width = 1500;
                        TeamPlayer.HorizontalAlignment = HorizontalAlignment.Stretch;

                        TeamPlayer.Kick.Click += Kick_Click;
                        TeamPlayer.Inviter.Click += async (sender, e) =>
                        {
                            LastSender = (Button)sender;
                            var s = (Member)LastSender.Tag;
                            await UserClient.calls.GrantInvite(s.SummonerId);
                            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                TeamPlayer.Inviter.Visibility = Visibility.Hidden;
                                TeamPlayer.UnInviter.Visibility = Visibility.Visible;
                            }));
                        };
                        TeamPlayer.UnInviter.Click += async (sender, e) =>
                        {
                            LastSender = (Button)sender;
                            var s = (Member)LastSender.Tag;
                            await UserClient.calls.RevokeInvite(s.SummonerId);
                            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                TeamPlayer.Inviter.Visibility = Visibility.Visible;
                                TeamPlayer.UnInviter.Visibility = Visibility.Hidden;
                            }));
                        };
                        TeamPlayer.Profile.Click += Profile_Click;
                        TeamPlayer.Owner.Click += Owner_Click;
                        Players++;

                        PublicSummoner Summoner = await UserClient.calls.GetSummonerByName(stats.SummonerName);

                        //Populate the ProfileIcon
                        int ProfileIconID = Summoner.ProfileIconId;

                        TeamPlayer.ProfileIcon.Source = new BitmapImage(Client.GetIconUri(ProfileIconID));

                        //Make it so you cant kick yourself
                        if (stats.SummonerName == UserClient.LoginPacket.AllSummonerData.Summoner.Name)
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
                        await UserClient.calls.CallLCDS(Guid.NewGuid().ToString(), "suggestedPlayers",
                            "retrieveOnlineFriendsOfFriends", "{\"queueId\":" + queueId + "}");
                    }
                }));
            }
            catch
            {
            }
        }

        private void Update_OnMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            if (message.Body.GetType() == typeof(LobbyStatus))
            {
                var Lobby = message.Body as LobbyStatus;
                CurrentLobby = Lobby;
                RenderLobbyData();
            }
            else if (message.Body is GameDTO)
            {
                var QueueDTO = message.Body as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    if (QueueDTO.GameState == "TERMINATED")
                    {
                        Client.HasPopped = false;
                        UserClient.RiotConnection.MessageReceived += GotQueuePop;
                    }
                }));
            }
            else if (message.Body is GameNotification)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    setStartButtonText("Start Game");
                    inQueue = false;
                    Client.inQueueTimer.Visibility = Visibility.Hidden;
                }));
            }
            else if (message.Body is SearchingForMatchNotification)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input,
                    new ThreadStart(() => { EnteredQueue(message.Body as SearchingForMatchNotification); }));
            }
            else if (message.Body is InvitePrivileges)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var priv = message.Body as InvitePrivileges;
                    if (priv.canInvite)
                    {
                        var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                        {
                            Text = Client.GetDictText("AllowInvites") + Environment.NewLine
                        };
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                        InviteButton.IsEnabled = true;
                    }
                    else
                    {
                        var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                        {
                            Text = Client.GetDictText("DisallowInvites") + Environment.NewLine
                        };
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                        InviteButton.IsEnabled = false;
                    }
                }));
            }
            else if (message.Body is LcdsServiceProxyResponse)
            {
                parseLcdsMessage((LcdsServiceProxyResponse) message.Body); //Don't look there, its ugly!!! :)))
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
                        var invitePlayer = new SuggestedPlayerItem {PlayerLabel = {Content = s.summonerName}};
                        invitePlayer.InviteButton.Click += async (obj, e) =>
                        {
                            await UserClient.calls.InviteFriendOfFriend(s.summonerId, s.commonFriendId);
                            foreach (SuggestedPlayerItem item in FriendsOfFriendsView.Items)
                            {
                                if ((string)item.PlayerLabel.Content == s.summonerName)
                                {
                                    item.InviteButton.IsEnabled = false;
                                    item.InviteButton.Content = Client.GetDictText("Invited");
                                    var t = new Timer {AutoReset = false};
                                    t.Elapsed += (source, args) =>
                                    {
                                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                                        {
                                            item.InviteButton.IsEnabled = true;
                                            item.InviteButton.Content = Client.GetDictText("Invite");
                                        }));
                                    };
                                    t.Interval = 5000;
                                    t.Start();
                                }
                            }
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
            await UserClient.calls.Leave();
            await UserClient.calls.PurgeFromQueues();
            inQueue = false;
//#pragma warning disable CS4014
            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => Client.inQueueTimer.Visibility = Visibility.Hidden));
//#pragma warning restore CS4014
            PingTimer.Stop();
            UserClient.GameStatus = "outOfGame";
            UserClient.SetChatHover();
            Client.SwitchPage(Client.MainPage);
            Client.ClearPage(typeof(TeamQueuePage));
            Client.ReturnButton.Visibility = Visibility.Hidden;
        }

        private void GotQueuePop(object sender, MessageReceivedEventArgs message)
        {
            if (!Client.HasPopped && message.Body is GameDTO && (message.Body as GameDTO).GameState == "JOINING_CHAMP_SELECT")
            {
                Client.HasPopped = true;
                var Queue = message.Body as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Client.OverlayContainer.Content = new QueuePopOverlay(Queue, this).Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
            }
            else if (message.Body is GameNotification && (message.Body as GameNotification).Type == "PLAYER_QUIT")
            {
                setStartButtonText("Start Game");
                inQueue = false;
                UserClient.GameStatus = "outOfGame";
                UserClient.SetChatHover();
                Dispatcher.Invoke(() =>
                {
                    Client.inQueueTimer.Visibility = Visibility.Hidden;
                    TeamListView.Opacity = 1D;
                });
            }
        }
        private void RestartDodgePop(object sender, object message)
        {
            if (message is GameDTO)
            {
                var queue = message as GameDTO;
                if (queue.GameState == "TERMINATED")
                {
                    Client.runonce = false;
                    UserClient.PlayerAccepedQueue += Client_PlayerAccepedQueue;
                }
            }
            else if (message is PlayerCredentialsDto)
            {
                UserClient.RiotConnection.MessageReceived -= RestartDodgePop;
            }
        }

        void Client_PlayerAccepedQueue(bool accept)
        {
            if (accept)
                UserClient.RiotConnection.MessageReceived += RestartDodgePop;
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (ChatTextBox.Text == "!~dev")
            {
                if (!UserClient.Dev)
                {
                    var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                    {
                        Text = Client.GetDictText("NotDev") + Environment.NewLine
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                }
                ChatTextBox.Text = "";
            }
            else
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = UserClient.LoginPacket.AllSummonerData.Summoner.Name + ": "
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (UserClient.Filter)
                    tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
                else
                    tr.Text = ChatTextBox.Text + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                if (string.IsNullOrEmpty(ChatTextBox.Text))
                    return;
                UserClient.XmppConnection.Send(new Message(jid, MessageType.groupchat, ChatTextBox.Text));
                ChatTextBox.Text = "";
                ChatText.ScrollToEnd();
            }
        }

        internal List<int> QueueIds;

        private bool inQueue;

#pragma warning disable 4014

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!inQueue)
            {
                parameters = new MatchMakerParams();
                parameters.Languages = null;
                QueueIds = new List<int>();
                QueueIds.Add(queueId);
                parameters.QueueIds = (QueueIds.ToArray());
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
                parameters.TeamId = selectedTeamId;
                parameters.BotDifficulty = botDifficulty;
                EnteredQueue(await UserClient.calls.AttachTeamToQueue(parameters));
            }
            else
            {
                UserClient.calls.PurgeFromQueues();
                setStartButtonText(Client.GetDictText("StartGame"));
                inQueue = false;
                UserClient.GameStatus = "outOfGame";
                UserClient.SetChatHover();
                Dispatcher.Invoke(() =>
                {
                    Client.inQueueTimer.Visibility = Visibility.Hidden;
                    TeamListView.Opacity = 1D;
                });
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
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    Client.HasPopped = false;
                    var messageOver = new MessageOverlay {MessageTitle = {Content = Client.GetDictText("FailedJoinQueue") }};
                    foreach (var item in result.PlayerJoinFailures)
                    {
                        var x = item;
                        TimeSpan time = TimeSpan.FromMilliseconds(x.PenaltyRemainingTime);
                        switch (x.ReasonFailed)
                        {
                            case "LEAVER_BUSTER_TAINTED_WARNING":
                                messageOver.MessageTextBox.Text += Client.GetDictText("LEAVER_BUSTER_TAINTED_WARNING");
                                //Need to implement their new warning for leaving.
                                break;
                            case "QUEUE_DODGER":
                                messageOver.MessageTextBox.Text += Client.GetDictText("QUEUE_DODGER").Replace("{USER}", x.Summoner.Name) + Environment.NewLine;
                                messageOver.MessageTextBox.Text += Client.GetDictText("QUEUE_DODGER_TIME").Replace("{TIME}", $"{time.Minutes:D2}m:{time.Seconds:D2}s");
                                break;
                            case "QUEUE_RESTRICTED":
                                messageOver.MessageTextBox.Text += Client.GetDictText("QUEUE_RESTRICTED");
                                messageOver.MessageTextBox.Text += Client.GetDictText("QUEUE_RESTRICTED_NOTE");
                                break;
                            case "RANKED_RESTRICTED":
                                messageOver.MessageTextBox.Text += Client.GetDictText("RANKED_RESTRICTED");
                                break;
                            case "RANKED_MIN_LEVEL":
                                messageOver.MessageTextBox.Text += Client.GetDictText("RANKED_MIN_LEVEL");
                                break;
                            case "QUEUE_PARTICIPANTS":
                                messageOver.MessageTextBox.Text += Client.GetDictText("QUEUE_PARTICIPANTS");
                                break;
                            case "LEAVER_BUSTED":
                                var xm = (BustedLeaver)x;
                                    Client.Log("LeaverBuster, Access token is: " + xm.AccessToken);
                                    var message = new MessageOverlay
                                    {
                                        MessageTitle = { Content = Client.GetDictText("LeaverBuster") },
                                        MessageTextBox = { Text = "" }
                                    };
                                    Timer t = new Timer { Interval = 1000 };
                                    var timeleft = xm.LeaverPenaltyMilisRemaining;
                                    t.Elapsed += (messafge, mx) =>
                                    {
                                        timeleft = timeleft - 1000;
                                        var timex = TimeSpan.FromMilliseconds(timeleft);
                                        Dispatcher.BeginInvoke(
                                            DispatcherPriority.Input, new ThreadStart(() =>
                                            {
                                                //Can not bypass this sadly, it just relaunches
                                                message.MessageTextBox.Text = Client.GetDictText("LeaverBusterAbandonMatchNote") + Environment.NewLine;
                                                message.MessageTextBox.Text += Client.GetDictText("QUEUE_DODGER_TIME").
                                                Replace("{TIME}", $"{timex.Minutes:D2}m:{timex.Seconds:D2}s") + Environment.NewLine;

                                                message.MessageTextBox.Text += Client.GetDictText("CloseStillInQueue");

                                                Client.OverlayContainer.Content = message.Content;
                                                if (!(timeleft < 0))
                                                {
                                                    return;
                                                }
                                                t.Stop();
                                                Client.OverlayContainer.Visibility = Visibility.Hidden;
                                            }));

                                    };
                                    t.Start();
                                    Client.OverlayContainer.Content = message.Content;
                                    Client.OverlayContainer.Visibility = Visibility.Visible;
                                if (CurrentLobby.Owner.SummonerId.MathRound() !=
                                    UserClient.LoginPacket.AllSummonerData.Summoner.SumId.MathRound())
                                {
                                    return;
                                }
                                EnteredQueue(await UserClient.calls.AttachTeamToQueue(parameters, new AsObject { { "LEAVER_BUSTER_ACCESS_TOKEN", xm.AccessToken } }));
                                break;
                            case "RANKED_NUM_CHAMPS":
                                messageOver.MessageTextBox.Text += Client.GetDictText("RANKED_NUM_CHAMPS");
                                break;
                            default:
                                messageOver.MessageTextBox.Text += Client.GetDictText("SubmitLeaverBusterUnknown").Replace("{REASON}", x.ReasonFailed);
                                break;
                        }
                    }
                    Client.OverlayContainer.Content = messageOver.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                return;
            }
            UserClient.RiotConnection.MessageReceived += GotQueuePop;
            setStartButtonText(Client.GetDictText("JoiningQueue"));
            startTime = 1;
            inQueue = true;
            UserClient.GameStatus = "inQueue";
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (Client.inQueueTimer.Visibility == Visibility.Hidden)
                    Client.inQueueTimer.Visibility = Visibility.Visible;
                TeamListView.Opacity = 0.3D;
            }));
            UserClient.timeStampSince = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalMilliseconds;
            UserClient.SetChatHover();
        }

        private void AutoAcceptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UserClient.AutoAcceptQueue = AutoAcceptCheckBox.IsChecked ?? false;
        }

        private void SelectChamp_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new SelectChampOverlay(this).Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        internal void CreateText(string text, SolidColorBrush color)
        {
            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = text + Environment.NewLine
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
        }

        private class SuggestedFriend
        {
            public double summonerId { get; set; }
            public double commonFriendId { get; set; }
            public string summonerName { get; set; }
            public string commonFriendName { get; set; }
            //public string SuggestedPlayerType { get; set; }
        }

        private void InstaCall_Click(object sender, RoutedEventArgs e)
        {
            if (ChatTextBox.Text == string.Empty)
            {
                CreateText(UserClient.InstaCall ? Client.GetDictText("InstaCallDisabled") : Client.GetDictText("InstaCallTypeCall"),
                    Brushes.OrangeRed);
                UserClient.InstaCall = false;
                return;
            }
            UserClient.InstaCall = true;
            UserClient.CallString = ChatTextBox.Text;
            CreateText(Client.GetDictText("InstaCallNotify").Replace("{CHAMP}", UserClient.CallString), Brushes.OrangeRed);
            ChatTextBox.Text = string.Empty;
        }

        public void VisualQueueLeave()
        {
            setStartButtonText(Client.GetDictText("StartGame"));
            inQueue = false;
            UserClient.GameStatus = "outOfGame";
            UserClient.SetChatHover();
            Dispatcher.Invoke(() =>
            {
                Client.inQueueTimer.Visibility = Visibility.Hidden;
                TeamListView.Opacity = 1D;
            });
        }
    }
}