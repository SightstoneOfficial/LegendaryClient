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

        internal LobbyStatus CurrentLobby;

        /// <summary>
        /// When invited to a team
        /// </summary>
        /// <param name="Message"></param>
        public TeamQueuePage(string InviteId, object message)
        {
            InitializeComponent();
            Client.InviteListView = InviteListView;
            Client.PVPNet.OnMessageReceived += Update_OnMessageReceived;
            //Client.PVPNet.Accept(InviteId.ToString());
            Client.OnMessage += Client_OnMessage;

            LobbyStatus Gamestats = message as LobbyStatus;
            CurrentLobby = Gamestats;

            i = 10;
            PingTimer = new Timer(1000);
            PingTimer.Elapsed += new ElapsedEventHandler(PingElapsed);
            PingTimer.Enabled = true;
            PingElapsed(1, null);

            string ObfuscatedName = Client.GetObfuscatedChatroomName(InviteId, ChatPrefixes.Arranging_Game);
            string JID = Client.GetChatroomJID(ObfuscatedName, "0", false);
            //string JID = "ag~43dc8f215b6b247460df83410b085c4f2b150bad@sec.pvp.net";
            newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
            newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            newRoom.OnRoomMessage += newRoom_OnRoomMessage;
            newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
            newRoom.Join();
        }

        internal void PingElapsed(object sender, ElapsedEventArgs e)
        {
            if (i++ < 10) //Ping every 10 seconds
                return;
            i = 0;
            double PingAverage = HighestPingTime(Client.Region.PingAddresses);
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
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

        private void Update_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(LobbyStatus))
            {                
                
                
                Client.InviteListView.Items.Clear();

                if (Client.LoginPacket.AllSummonerData.Summoner.Name == CurrentLobby.Owner);
                {
                    IsOwner = true;
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
                InvitePlayer invitePlayer = new InvitePlayer();
                invitePlayer.StatusLabel.Content = CurrentLobby.inviteeState;
                invitePlayer.PlayerLabel.Content = CurrentLobby.SummonerName;
                Client.InviteListView.Items.Add(invitePlayer);
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

        private void Leave_Click(object sender, RoutedEventArgs e)
        {
            Client.PVPNet.Leave();
            Client.SwitchPage(new MainPage());
            Client.ClearPage(new TeamQueuePage(null, null));
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                TeamListView.Items.Add(participant.NickJID);
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
        {
            
            
        }

        private void ParseCurrentInvitees(string Message)
        {
            
            
            /*
            using (XmlReader reader = XmlReader.Create(new StringReader(Message)))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "invitee":
                                InvitePlayer invitePlayer = new InvitePlayer();
                                invitePlayer.StatusLabel.Content = Client.TitleCaseString(reader.GetAttribute("status"));
                                invitePlayer.PlayerLabel.Content = reader.GetAttribute("name");
                                Client.InviteListView.Items.Add(invitePlayer);
                                break;
                        }
                    }
                }
            }*/
        }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            //GameQueueConfig config = (GameQueueConfig)LastSender.Tag;
            MatchMakerParams parameters = new MatchMakerParams();
            //parameters.QueueIds = new Int32[] { Convert.ToInt32(config.Id) };
            Client.PVPNet.AttachToQueue(parameters, new SearchingForMatchNotification.Callback(EnteredQueue));
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
                        message.MessageTextBox.Text = "Unable to join the queue due to you recently dodging a game." + Environment.NewLine;
                        TimeSpan time = TimeSpan.FromMilliseconds(result.PlayerJoinFailures[0].PenaltyRemainingTime);
                        message.MessageTextBox.Text = "You have " + string.Format("{0:D2}m:{1:D2}s", time.Minutes, time.Seconds) + " remaining until you may queue again";
                    }
                    else if (result.PlayerJoinFailures[0].ReasonFailed == "RANKED_MIN_LEVEL")
                    {
                        message.MessageTextBox.Text = "You do not meet the requirements for this queue." + Environment.NewLine;
                    }
                    else if (result.PlayerJoinFailures[0].ReasonFailed == "QUEUE_PARTICIPANTS")
                    {
                        message.MessageTextBox.Text = "This queue is in dev. Please use this queue on the real league of legends client." + Environment.NewLine;
                    }
                    Client.OverlayContainer.Content = message.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                return;
            }
        }
        private void GotQueuePop(object sender, object message)
        {
            GameDTO Queue = message as GameDTO;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Client.OverlayContainer.Content = new QueuePopOverlay(Queue).Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
            }));
            Client.PVPNet.OnMessageReceived -= GotQueuePop;
        }

    }
}