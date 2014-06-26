using LegendaryClient.Controls;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for PlayPage.xaml
    /// </summary>
    public partial class PlayPage : Page
    {
        private int i = 0;
        private static Timer PingTimer;
        private Dictionary<double, JoinQueue> configs = new Dictionary<double, JoinQueue>();
        private Dictionary<Button, int> ButtonTimers = new Dictionary<Button, int>();
        private List<double> Queues = new List<double>();
        //JoinQueue item = new JoinQueue();

        public PlayPage()
        {
            InitializeComponent();
            Client.IsOnPlayPage = true;
            i = 10;
            PingTimer = new Timer(1000);
            PingTimer.Elapsed += new ElapsedEventHandler(PingElapsed);
            PingTimer.Enabled = true;
            PingElapsed(1, null);
        }

        private void PingRectangle_MouseDown(object sender, ElapsedEventArgs e)
        {
            //Client.SwitchPage(new TeamQueuePage());
        }
        internal void PingElapsed(object sender, ElapsedEventArgs e)
        {
            //TeambuilderCorrect();

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var keys = new List<Button>(ButtonTimers.Keys);
                foreach (Button pair in keys)
                {
                    ButtonTimers[pair] = ButtonTimers[pair] + 1;
                    TimeSpan time = TimeSpan.FromSeconds(ButtonTimers[pair]);
                    Button realButton = (Button)pair.Tag;
                    realButton.Content = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
                }
            }));
            if (i++ < 10) //Ping every 10 seconds
                return;
            i = 0;
            if (!Client.IsOnPlayPage)
                return;
            double PingAverage = HighestPingTime(Client.Region.PingAddresses);
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                //Ping
                TeambuilderCorrect();
                PingLabel.Content = Math.Round(PingAverage).ToString() + "ms";
                if (PingAverage == 0)
                {
                    PingLabel.Content = "Timeout";
                }
                if (PingAverage == -1)
                {
                    PingLabel.Content = "Ping not enabled for this region";
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

                //Queues
                GameQueueConfig[] OpenQueues = await Client.PVPNet.GetAvailableQueues();
                Array.Sort(OpenQueues, delegate(GameQueueConfig config, GameQueueConfig config2)
                {
                    return config.CacheName.CompareTo(config2.CacheName);
                });
                foreach (GameQueueConfig config in OpenQueues)
                {
                    JoinQueue item = new JoinQueue();
                    if (configs.ContainsKey(config.Id))
                    {
                        item = configs[config.Id];
                    }
                    item.Height = 80;
                    item.QueueButton.Tag = config;
                    item.QueueButton.Click += QueueButton_Click;
                    item.TeamQueueButton.Click += TeamQueueButton_Click;
                    item.TBCreateBotton.Click += TBCreateBotton_Click;
                    item.TBSearchButton.Click += TBSearchButton_Click;
                    item.QueueLabel.Content = Client.InternalQueueToPretty(config.CacheName);
                    QueueInfo t = await Client.PVPNet.GetQueueInformation(config.Id);
                    item.AmountInQueueLabel.Content = "People in queue: " + t.QueueLength;
                    TimeSpan time = TimeSpan.FromMilliseconds(t.WaitTime);
                    string answer = string.Format("{0:D2}m:{1:D2}s", time.Minutes, time.Seconds);
                    item.WaitTimeLabel.Content = "Avg Wait Time: " + answer;
                    if (!configs.ContainsKey(config.Id))
                    {
                        configs.Add(config.Id, item);
                        QueueListView.Items.Add(item);
                    }
                        //GROUPFINDER
                }
            }));
        }


        /// <summary>
        /// Queue bool
        /// </summary>
        private Button LastSender;
        
        //Duo
        public static void TBCreateBotton_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new TeamBuilder(true));
        }
        //Solo
        public static void TBSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new TeamBuilder(false));
        }
        private async void TeamQueueButton_Click(object sender, RoutedEventArgs e)
        {
            
            //To leave all other queues
            {
            var keys = new List<Button>(ButtonTimers.Keys);
            foreach (Button pair in keys)
            {
                Button realButton = (Button)pair.Tag;
                realButton.Content = "Queue";
            }
            ButtonTimers = new Dictionary<Button, int>();
            Queues = new List<double>();
            await Client.PVPNet.PurgeFromQueues();
            }
            //To Start Queueing
            LastSender = (Button)sender;
            GameQueueConfig config = (GameQueueConfig)LastSender.Tag;
            if (Queues.Contains(config.Id))
            {
                return;
            }
            Queues.Add(config.Id);
            MatchMakerParams parameters = new MatchMakerParams();
            parameters.QueueIds = new Int32[] { Convert.ToInt32(config.Id) };
            await Client.PVPNet.createArrangedTeamLobby(Convert.ToInt32(config.Id));
            //Client.PVPNet.AttachToQueue(parameters, new SearchingForMatchNotification.Callback(EnteredQueue));
            Client.SwitchPage(new TeamQueuePage(true));
        }

        private void TeambuilderCorrect()
        {
            JoinQueue item = new JoinQueue();
            if (item.QueueLabel.Content == "matching-queue-GROUPFINDER-5x5-game-queue")
            {
                 item.TeamQueueButton.Visibility = Visibility.Hidden;
                 item.QueueButton.Visibility = Visibility.Hidden;
                 item.QueueLabel.Content = "Team Builder";
                 item.AmountInQueueLabel.Visibility = Visibility.Hidden;
                 item.WaitTimeLabel.Visibility = Visibility.Hidden;
            }
        }
        private async void QueueButton_Click(object sender, RoutedEventArgs e)
        {
            //To leave all other queues
            {
            var keys = new List<Button>(ButtonTimers.Keys);
            foreach (Button pair in keys)
            {
                Button realButton = (Button)pair.Tag;
                realButton.Content = "Queue";
            }
            ButtonTimers = new Dictionary<Button, int>();
            Queues = new List<double>();
            await Client.PVPNet.PurgeFromQueues();
            }
            //To Start Queueing
            LastSender = (Button)sender;
            GameQueueConfig config = (GameQueueConfig)LastSender.Tag;
            if (Queues.Contains(config.Id))
            {
                return;
            }
            Queues.Add(config.Id);
            MatchMakerParams parameters = new MatchMakerParams();
            parameters.QueueIds = new Int32[] { Convert.ToInt32(config.Id) };
            //await Client.PVPNet.createArrangedTeamLobby(Convert.ToInt32(config.Id));
            Client.PVPNet.AttachToQueue(parameters, new SearchingForMatchNotification.Callback(EnteredQueue));
            //Client.SwitchPage(new TeamQueuePage(true));
        }

        private void EnteredQueue(SearchingForMatchNotification result)
        {
            if (result.PlayerJoinFailures != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Button item = LastSender;
                    GameQueueConfig config = (GameQueueConfig)item.Tag;
                    Queues.Remove(config.Id);
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

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Button item = LastSender;
                Button fakeButton = new Button(); //We require a unique button to add to the dictionary
                fakeButton.Tag = item;
                item.Content = "00:00";
                ButtonTimers.Add(fakeButton, 0);
            }));
            Client.PVPNet.OnMessageReceived += GotQueuePop;
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

        private void CreateCustomGameButton_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new CreateCustomGamePage());
        }

        private void JoinCustomGameButton_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new CustomGameListingPage());
        }

        private void AutoAcceptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Client.AutoAcceptQueue = (AutoAcceptCheckBox.IsChecked.HasValue) ? AutoAcceptCheckBox.IsChecked.Value : false;
        }

        private async void LeaveQueuesButton_Click(object sender, RoutedEventArgs e)
        {
            var keys = new List<Button>(ButtonTimers.Keys);
            foreach (Button pair in keys)
            {
                Button realButton = (Button)pair.Tag;
                realButton.Content = "Queue";
            }
            ButtonTimers = new Dictionary<Button, int>();
            Queues = new List<double>();
            await Client.PVPNet.PurgeFromQueues();
        }
    }
}