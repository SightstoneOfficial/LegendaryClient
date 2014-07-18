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

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for TeamQueuePage.xaml
    /// </summary>
    public partial class TeamQueuePage : Page
    {
        Message MessageData;
        //long InviteId = 0;
        private Room newRoom;
        bool IsOwner = false;
        private Button LastSender;
        private int i = 0;
        private static Timer PingTimer;

        //gamemetadata
        int queueId;
        bool isRanked;
        string rankedTeamName;
        int mapId;
        int gameTypeConfigId;
        string gameMode;
        string gameType;

        string Invite;

        internal static LobbyStatus CurrentLobby;

        /// <summary>
        /// When invited to a team
        /// </summary>
        /// <param name="Message"></param>
        public TeamQueuePage(string Invid)
        {
            InitializeComponent();
            Client.InviteListView = InviteListView;
            Client.PVPNet.OnMessageReceived += Update_OnMessageReceived;
            Client.OnMessage += Client_OnMessage;
            MainWindow Window = new MainWindow();
            Window.Hide();
            Invite = Invid;
            
            LoadStats();
        }

        public async void LoadStats()
        { 

            ///Wow Riot, you get rid of getLobbyStatus so I have to do this all over -.-
            LobbyStatus Lobby = await Client.PVPNet.getLobbyStatus(Invite);
            CurrentLobby = Lobby;

            //arrangeing_game
            string ObfuscatedName = Client.GetObfuscatedChatroomName(CurrentLobby.InvitationID.Replace("INVID", "invid"), ChatPrefixes.Arranging_Game); //Why do you need to replace INVID with invid Riot?
            string JID = Client.GetChatroomJID(ObfuscatedName, CurrentLobby.ChatKey, false);
            newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
            newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            newRoom.OnRoomMessage += newRoom_OnRoomMessage;
            newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
            newRoom.Join(CurrentLobby.ChatKey);

            i = 10;
            PingTimer = new Timer(1000);
            PingTimer.Elapsed += new ElapsedEventHandler(PingElapsed);
            PingTimer.Enabled = true;
            PingElapsed(1, null);
            InviteButton.IsEnabled = false;
            StartGameButton.IsEnabled = false;

            ///Way smarter way then just putting the code here
            RenderLobbyData();
        }
        
        private async void Inviter_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            Member stats = (Member)LastSender.Tag;
            Client.PVPNet.GrantInvite(stats.SummonerId);

        }
        private async void Profile_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            Member stats = (Member)LastSender.Tag;
            Client.SwitchPage(new ProfilePage(stats.SummonerName));
        }
        private async void Kick_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            Member stats = (Member)LastSender.Tag;
            Client.PVPNet.Kick(stats.SummonerId);
        }
        private async void Owner_Click(object sender, RoutedEventArgs e)
        {
            LastSender = (Button)sender;
            Member stats = (Member)LastSender.Tag;
            Client.PVPNet.MakeOwner(stats.SummonerId);
        }

        internal void PingElapsed(object sender, ElapsedEventArgs e)
        {
            if (i++ < 10) //Ping every 10 seconds
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
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Client.InviteListView.Items.Clear();
                TeamListView.Items.Clear();
                if (Client.LoginPacket.AllSummonerData.Summoner.Name == CurrentLobby.Owner);
                {
                    IsOwner = true;
                }
                if (Client.LoginPacket.AllSummonerData.Summoner.Name != CurrentLobby.Owner);
                {
                    IsOwner = false;
                }

                if (IsOwner == true)
                {
                    InviteButton.IsEnabled = true;
                    StartGameButton.IsEnabled = true;
                }
                else if (IsOwner == false)
                {
                    InviteButton.IsEnabled = false;
                    StartGameButton.IsEnabled = false;
                }

                foreach (Member stats in CurrentLobby.Members)
                {
                    TeamControl TeamPlayer = new TeamControl();
                    TeamPlayerStats = TeamPlayer;
                    TeamPlayer.Name.Content = stats.SummonerName;
                    TeamPlayer.SumID.Content = stats.SummonerName;
                    TeamPlayer.Kick.Tag = stats;
                    TeamPlayer.Inviter.Tag = stats;
                    TeamPlayer.Profile.Tag = stats;
                    TeamPlayer.Owner.Tag = stats;

                    TeamPlayer.Kick.Click += Kick_Click;
                    TeamPlayer.Inviter.Click += Inviter_Click;
                    TeamPlayer.Profile.Click += Profile_Click;
                    TeamPlayer.Owner.Click += Owner_Click;

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
                        if (IsOwner == false)
                        {
                            //So you don't crash trying to kick someone when you cant
                            TeamPlayer.Kick.Visibility = Visibility.Hidden;
                            TeamPlayer.Inviter.Visibility = Visibility.Hidden;
                            TeamPlayer.Owner.Visibility = Visibility.Hidden;
                        }
                    }
                    TeamListView.Items.Add(TeamPlayer);
                }

                foreach (Invitee statsx in CurrentLobby.Invitees)
                {
                    InvitePlayer invitePlayer = new InvitePlayer();
                    invitePlayer.StatusLabel.Content = statsx.inviteeState;
                    invitePlayer.PlayerLabel.Content = statsx.SummonerName;
                    Client.InviteListView.Items.Add(invitePlayer);
                }
            }));
        }

        private void Update_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(LobbyStatus))
            {
                LobbyStatus Lobby = message as LobbyStatus;
                CurrentLobby = Lobby;
                RenderLobbyData();
            }
            else if (message.GetType() == typeof(GameDTO) && Client.runonce == false)
            {
                GameDTO Queue = message as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Client.OverlayContainer.Content = new QueuePopOverlay(Queue).Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                Client.runonce = true;
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
            Client.SwitchPage(new MainPage());
            Client.ClearPage(new TeamQueuePage(null));
        }


        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                //TeamListView.Items.Add(participant.NickJID);
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
                }
            }));
        }

        private async void Client_OnMessage(object sender, Message msg)
        { /*Not needed anymore */ }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            MatchMakerParams parameters = new MatchMakerParams();
            parameters.QueueIds = new Int32[] { Convert.ToInt32(queueId) };
            parameters.InvitationId = CurrentLobby.InvitationID;
            parameters.Team = null;
            
            Client.PVPNet.AttachTeamToQueue(parameters, new SearchingForMatchNotification.Callback(EnteredQueue));
        }



        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            tr.Text = ChatTextBox.Text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            newRoom.PublicMessage(ChatTextBox.Text);
            ChatTextBox.Text = "";
        }
        private void EnteredQueue(SearchingForMatchNotification result)
        {
            if (result.PlayerJoinFailures != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    MessageOverlay message = new MessageOverlay();
                    message.MessageTitle.Content = "Failed to join queue";
                    message.MessageTextBox.Text = result.PlayerJoinFailures[0].ReasonFailed;
                    if (result.PlayerJoinFailures[0].ReasonFailed == "QUEUE_DODGER")
                    {
                        message.MessageTextBox.Text = "Unable to join the queue due to a player recently dodging a game." + Environment.NewLine;
                        TimeSpan time = TimeSpan.FromMilliseconds(result.PlayerJoinFailures[0].PenaltyRemainingTime);
                        message.MessageTextBox.Text = "You have " + string.Format("{0:D2}m:{1:D2}s", time.Minutes, time.Seconds) + " remaining until you may queue again";
                    }
                    Client.OverlayContainer.Content = message.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                return;
            }
        }
        private void GotQueuePop(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                GameDTO Queue = message as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Client.OverlayContainer.Content = new QueuePopOverlay(Queue).Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                Client.PVPNet.OnMessageReceived -= GotQueuePop;

            }
            else if (message.GetType() == typeof(LobbyStatus))
            {
                LobbyStatus mg = message as LobbyStatus;
                {
                    CurrentLobby = mg;
                }
            }
        }

    }
}