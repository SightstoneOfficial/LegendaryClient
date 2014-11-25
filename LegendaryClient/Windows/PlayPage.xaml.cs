using LegendaryClient.Controls;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
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
        private Dictionary<Button, int> ButtonTimers = new Dictionary<Button, int>();
        private GameSeperator[] seperators = new GameSeperator[3];
        private List<double> Queues = new List<double>();
        private bool InQueue = false;
        public static bool DoneLoading = false;
        private int currentAmount = 0;
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

        private bool RunOnce = false;

        internal void PingElapsed(object sender, ElapsedEventArgs e)
        {
            //TeambuilderCorrect();

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var keys = new List<Button>(ButtonTimers.Keys);
                foreach (Button pair in keys)
                {
                    ButtonTimers[pair]++;
                    TimeSpan time = TimeSpan.FromSeconds(ButtonTimers[pair]);
                    Button realButton = (Button)pair.Tag;
                    realButton.Content = string.Format("{0:D2}:{1:D2} Re-Click To Leave", time.Minutes, time.Seconds);
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
                if (!RunOnce)
                {
                    RunOnce = true;
                    WaitingForQueues.Visibility = Visibility.Visible;
                    for (int b = 0; b < 3; b++)
                    {
                        seperators[b] = new GameSeperator(QueueListView);
                        seperators[b].Height = 80;
                        switch (b)
                        {
                            case 0:
                                seperators[b].QueueLabel.Content = Client.InternalQueueToPretty("Bot Queues");
                                seperators[b].Tag = "Bot";
                                break;
                            case 1:
                                seperators[b].QueueLabel.Content = Client.InternalQueueToPretty("Normal Queues");
                                seperators[b].Tag = "Normal";
                                break;
                            case 2:
                                seperators[b].QueueLabel.Content = Client.InternalQueueToPretty("Ranked Queues");
                                seperators[b].Tag = "Ranked";
                                break;
                        }
                        QueueListView.Items.Add(seperators[b]);

                    }
                    //Ping
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
                        item.Height = 80;
                        item.QueueButton.Tag = config;
                        item.QueueButton.Click += QueueButton_Click;
                        item.QueueButton.IsEnabled = false;
                        item.QueueButton.Content = "Use Team Page";
                        item.TeamQueueButton.Tag = config;
                        item.TeamQueueButton.Click += TeamQueueButton_Click;
                        item.QueueLabel.Content = Client.InternalQueueToPretty(config.CacheName);
                        item.queueID = config.Id;
                        QueueInfo t = await Client.PVPNet.GetQueueInformation(config.Id);
                        item.AmountInQueueLabel.Content = "People in queue: " + t.QueueLength;
                        TimeSpan time = TimeSpan.FromMilliseconds(t.WaitTime);
                        string answer = string.Format("{0:D2}m:{1:D2}s", time.Minutes, time.Seconds);
                        item.WaitTimeLabel.Content = "Avg Wait Time: " + answer;
                        if (config.TypeString == "BOT" || config.TypeString == "BOT_3x3")
                            seperators[0].Add(item);
                        else if (config.TypeString.StartsWith("RANKED_"))
                            seperators[2].Add(item);
                        else
                            seperators[1].Add(item);

                        switch (Client.InternalQueueToPretty(config.CacheName))
                        {
                            case "Teambuilder 5v5 Beta (In Dev. Do Not Play)":
                                item.QueueButton.IsEnabled = false;
                                item.TeamQueueButton.IsEnabled = false;
                                break;
                            case "Ranked Team 5v5":
                                item.QueueButton.IsEnabled = false;
                                break;
                            case "Ranked Team 3v3":
                                item.QueueButton.IsEnabled = false;
                                break;
                        }
                        currentAmount++;
                        if (currentAmount == OpenQueues.Length)
                        {
                            WaitingForQueues.Visibility = Visibility.Hidden;
                            foreach (GameSeperator seperator in seperators)
                                seperator.UpdateLabels();
                            DoneLoading = true;
                        }
                    }

                }
                else if (seperators[seperators.Length - 1] != null)
                    foreach (GameSeperator seperator in seperators)
                        seperator.UpdateLabels();
            }));
        }


        /// <summary>
        /// Queue bool
        /// </summary>
        private Button LastSender;

        private async void TeamQueueButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInGame()) return;
            //To leave all other queues
            await LeaveAllQueues();
            InQueue = false;
            LastSender = (Button)sender;
            GameQueueConfig config = (GameQueueConfig)LastSender.Tag;
            //Make Teambuilder work for duo
            if (config.Id != 61 && config.TypeString != "BOT")
            {
                if (Queues.Contains(config.Id))
                {
                    return;
                }
                Queues.Add(config.Id);
                MatchMakerParams parameters = new MatchMakerParams();
                parameters.QueueIds = new Int32[] { Convert.ToInt32(config.Id) };
                Client.GameQueue = Convert.ToInt32(config.Id);
                LobbyStatus Lobby = await Client.PVPNet.createArrangedTeamLobby(Convert.ToInt32(config.Id));

                Client.ClearPage(typeof(TeamQueuePage));
                Client.SwitchPage(new TeamQueuePage(Lobby.InvitationID, Lobby));
            }
            else if (config.TypeString == "BOT")
            {

            }
            else
            {
                LobbyStatus Lobby = await Client.PVPNet.createArrangedTeamLobby(Convert.ToInt32(config.Id));
                Client.SwitchPage(new TeamBuilderPage(true, Lobby));
            }

        }

        private async void QueueButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInGame()) return;
            //to queue
            if (InQueue == false)
            {
                LastSender = (Button)sender;
                GameQueueConfig config = (GameQueueConfig)LastSender.Tag;
                //Make TeamBuilder Work for solo
                if (config.Id != 61)
                {
                    if (Queues.Contains(config.Id))
                    {
                        return;
                    }
                    Queues.Add(config.Id);
                    MatchMakerParams parameters = new MatchMakerParams();
                    parameters.QueueIds = new Int32[] { Convert.ToInt32(config.Id) };
                    Client.QueueId = config.Id;
                    Client.PVPNet.AttachToQueue(parameters, new SearchingForMatchNotification.Callback(EnteredQueue));
                }
                else if (config.Id == 61)
                {

                    LobbyStatus Lobby = await Client.PVPNet.createArrangedTeamLobby(Convert.ToInt32(config.Id));
                    Client.ClearPage(typeof(TeamBuilderPage));
                    Client.SwitchPage(new TeamBuilderPage(false, Lobby));
                }
                return;
            }
            else if (InQueue == true)
            {
                InQueue = false;
                await LeaveAllQueues();
            }
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
            InQueue = true;
            Client.PVPNet.OnMessageReceived += GotQueuePop;
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
            if (isInGame()) return;
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.SwitchPage(new CreateCustomGamePage());
        }
        private void JoinFactionGameButton_Click(object sender, object e)
        {
            if (isInGame()) 
                return;
            Client.ClearPage(typeof(FactionsJoinGamePage));
            Client.SwitchPage(new FactionsJoinGamePage());
        }

        private bool isInGame()
        {
            if (Client.IsInGame)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    MessageOverlay message = new MessageOverlay();
                    message.MessageTitle.Content = "Failed to join queue";
                    message.MessageTextBox.Text = "You are currently in a game, if you need to reconnect please return to the reconnect page above!";
                    Client.OverlayContainer.Content = message.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                return true;
            }
            return false;
        }

        private void JoinCustomGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInGame()) return;
            Client.ClearPage(typeof(CustomGameListingPage));
            Client.SwitchPage(new CustomGameListingPage());
        }

        private void AutoAcceptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Client.AutoAcceptQueue = (AutoAcceptCheckBox.IsChecked.HasValue) ? AutoAcceptCheckBox.IsChecked.Value : false;
        }

        private async void LeaveQueuesButton_Click(object sender, RoutedEventArgs e)
        {
            await LeaveAllQueues();
        }

        private async Task<bool> LeaveAllQueues()
        {
            InQueue = false;
            await Client.PVPNet.PurgeFromQueues();
            await Client.PVPNet.QuitGame();
            await Client.PVPNet.Leave();
            Client.ClearPage(typeof(CustomGameLobbyPage));
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.ClearPage(typeof(FactionsCreateGamePage));
            Client.ClearPage(typeof(FactionsGameLobbyPage));
            Client.ClearPage(typeof(ChampSelectPage));

            foreach (Button btn in ButtonTimers.Keys)
            {
                Button realButton = (Button)btn.Tag;
                realButton.Content = "Queue";
            }
            ButtonTimers.Clear();

            Queues.Clear();
            return true;
        }

        private void CreateFactionGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (isInGame()) return;
            Client.ClearPage(typeof(FactionsCreateGamePage));
            Client.SwitchPage(new FactionsCreateGamePage());
        }
    }
}