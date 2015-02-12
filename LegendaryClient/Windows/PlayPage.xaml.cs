#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using Timer = System.Timers.Timer;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for PlayPage.xaml
    /// </summary>
    public partial class PlayPage
    {
        private static Timer PingTimer;
        public static bool DoneLoading = false;
        private readonly Dictionary<Button, int> ButtonTimers = new Dictionary<Button, int>();
        private readonly List<double> Queues = new List<double>();
        private readonly GameSeperator[] seperators = new GameSeperator[3];
        private bool InQueue;

        /// <summary>
        ///     Queue bool
        /// </summary>
        private Button LastSender;

        private bool RunOnce;

        private int currentAmount;
        private int i;
        //JoinQueue item = new JoinQueue();

        public PlayPage()
        {
            InitializeComponent();
            Client.PlayerAccepedQueue += Client_PlayerAccepedQueue;
            Change();

            Client.IsOnPlayPage = true;
            i = 10;
            PingTimer = new Timer(1000);
            PingTimer.Elapsed += PingElapsed;
            PingTimer.Enabled = true;
            PingElapsed(1, null);
        }

        private async void Client_PlayerAccepedQueue(bool accept)
        {
            if (accept)
                Client.PVPNet.OnMessageReceived += RestartDodgePop;
            else
                await LeaveAllQueues();
        }
        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

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
                    var realButton = (Button) pair.Tag;
                    realButton.Content = string.Format("{0:D2}:{1:D2} Re-Click To Leave", time.Minutes, time.Seconds);
                }
            }));
            if (i++ < 10) //Ping every 10 seconds
                return;

            i = 0;
            if (!Client.IsOnPlayPage)
                return;

            double pingAverage = HighestPingTime(Client.Region.PingAddresses);
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                if (!RunOnce)
                {
                    RunOnce = true;
                    WaitingForQueues.Visibility = Visibility.Visible;
                    for (int b = 0; b < 3; b++)
                    {
                        seperators[b] = new GameSeperator(QueueListView)
                        {
                            Height = 80
                        };
                        switch (b)
                        {
                            case 0:
                                seperators[b].QueueLabel.Content = "Bot Queues";
                                seperators[b].Tag = "Bot";
                                break;
                            case 1:
                                seperators[b].QueueLabel.Content = "Normal Queues";
                                seperators[b].Tag = "Normal";
                                break;
                            case 2:
                                seperators[b].QueueLabel.Content = "Ranked Queues";
                                seperators[b].Tag = "Ranked";
                                break;
                        }
                        QueueListView.Items.Add(seperators[b]);
                    }
                    //Ping
                    PingLabel.Content = Math.Round(pingAverage) + "ms";
                    if (pingAverage == 0)
                        PingLabel.Content = "Timeout";

                    if (pingAverage == -1)
                        PingLabel.Content = "Ping not enabled for this region";

                    var bc = new BrushConverter();
                    var brush = (Brush) bc.ConvertFrom("#FFFF6767");
                    if (pingAverage > 999 || pingAverage < 1)
                        PingRectangle.Fill = brush;

                    brush = (Brush) bc.ConvertFrom("#FFFFD667");
                    if (pingAverage > 110 && pingAverage < 999)
                        PingRectangle.Fill = brush;

                    brush = (Brush) bc.ConvertFrom("#FF67FF67");
                    if (pingAverage < 110 && pingAverage > 1)
                        PingRectangle.Fill = brush;

                    //Queues
                    GameQueueConfig[] openQueues = await Client.PVPNet.GetAvailableQueues();
                    Array.Sort(openQueues,
                        (config, config2) =>
                            String.Compare(config.CacheName, config2.CacheName, StringComparison.Ordinal));
                    foreach (GameQueueConfig config in openQueues)
                    {
                        QueueButtonConfig settings = new QueueButtonConfig();
                        settings.GameQueueConfig = config;

                        if(config.CacheName.Contains("INTRO"))
                            settings.BotLevel = "INTRO";
                        else if (config.CacheName.Contains("EASY") || config.Id == 25 || config.Id == 52)
                            settings.BotLevel = "EASY";
                        else if (config.CacheName.Contains("MEDIUM"))
                            settings.BotLevel = "MEDIUM";

                        var item = new JoinQueue
                        {
                            Height = 80,
                            QueueButton = {Tag = settings}
                        };
                        
                        item.QueueButton.Click += QueueButton_Click;
                        //item.QueueButton.IsEnabled = false;
                        item.QueueButton.Content = "Queue (Beta)";
                        item.TeamQueueButton.Tag = settings;
                        item.TeamQueueButton.Click += TeamQueueButton_Click;
                        item.QueueLabel.Content = Client.InternalQueueToPretty(config.CacheName);
                        item.QueueId = config.Id;
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
                                //item.TeamQueueButton.IsEnabled = false;
                                break;
                            case "Ranked Team 5v5":
                                item.QueueButton.IsEnabled = false;
                                break;
                            case "Ranked Team 3v3":
                                item.QueueButton.IsEnabled = false;
                                break;
                        }

                        if (item.QueueId == 25 || item.QueueId == 52)   //TT and Dominion: easy and medium bots have the same QueueId
                        {
                            settings.BotLevel = "MEDIUM";
                            var item2 = new JoinQueue
                            {
                                Height = 80,
                                QueueButton = { Tag = settings }
                            };
                            item2.QueueButton.Click += QueueButton_Click;
                            item2.QueueButton.Content = "Queue (Beta)";
                            item2.TeamQueueButton.Tag = settings;
                            item2.TeamQueueButton.Click += TeamQueueButton_Click;
                            item2.QueueLabel.Content = item.QueueLabel.Content.ToString().Replace("Easy", "Medium");
                            item2.AmountInQueueLabel.Content = "People in queue: " + t.QueueLength;
                            item2.WaitTimeLabel.Content = "Avg Wait Time: " + answer;

                            seperators[0].Add(item2);

                            if(!Client.Dev)
                                item2.QueueButton.IsEnabled = false;
                        }
                        
                        currentAmount++;
                        if (currentAmount != openQueues.Length)
                            continue;

                        WaitingForQueues.Visibility = Visibility.Hidden;
                        foreach (GameSeperator seperator in seperators)
                            seperator.UpdateLabels();

                        DoneLoading = true;
                    }
                }
                else if (seperators[seperators.Length - 1] != null)
                    foreach (GameSeperator seperator in seperators)
                        seperator.UpdateLabels();
            }));
        }


        private async void TeamQueueButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInGame()) return;
            //To leave all other queues
            await LeaveAllQueues();
            InQueue = false;
            LastSender = (Button) sender;
            var settings = (QueueButtonConfig) LastSender.Tag;
            var config = (GameQueueConfig) settings.GameQueueConfig;
            //Make Teambuilder work for duo
            if (config.Id == 41 || config.Id == 42)
            {
                if (Queues.Contains(config.Id))
                    return;

                Queues.Add(config.Id);
                Client.QueueId = config.Id;
                TeamSelect teamSelectWindow = new TeamSelect();

                Client.FullNotificationOverlayContainer.Content = teamSelectWindow.Content;
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
            }
            else if (config.Id != 61 && !config.TypeString.Contains("BOT"))
            {
                if (Queues.Contains(config.Id))
                    return;

                Queues.Add(config.Id);
                Client.QueueId = config.Id;
                LobbyStatus lobby = await Client.PVPNet.createArrangedTeamLobby(config.Id);

                Client.ClearPage(typeof (TeamQueuePage));
                Client.SwitchPage(new TeamQueuePage(lobby.InvitationID, lobby));
            }
            else if (config.TypeString.Contains("BOT"))
            {
                Queues.Add(config.Id);
                LobbyStatus lobby = await Client.PVPNet.createArrangedBotTeamLobby(config.Id, settings.BotLevel);

                Client.ClearPage(typeof(TeamQueuePage));
                Client.SwitchPage(new TeamQueuePage(lobby.InvitationID, lobby, false, null, settings.BotLevel));
            }
            else
            {
                LobbyStatus lobby = await Client.PVPNet.createArrangedTeamLobby(config.Id);
                Client.SwitchPage(new TeamBuilderPage(true, lobby));
            }
        }

        private async void QueueButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInGame())
                return;
            //to queue
            if (InQueue == false)
            {
                LastSender = (Button)sender; 
                var settings = (QueueButtonConfig)LastSender.Tag;
                var config = (GameQueueConfig)settings.GameQueueConfig;
                //Make TeamBuilder Work for solo
                if(config.TypeString.Contains("BOT"))
                {
                    if (Queues.Contains(config.Id))
                        return;

                    Queues.Add(config.Id);
                    var parameters = new MatchMakerParams
                    {
                        QueueIds = new[]
                        {
                            Convert.ToInt32(config.Id)
                        },
                        BotDifficulty = settings.BotLevel
                    };
                    Client.QueueId = config.Id;
                    Client.PVPNet.AttachToQueue(parameters, EnteredQueue);
                }
                else if (config.Id != 61)
                {
                    if (Queues.Contains(config.Id))
                        return;

                    Queues.Add(config.Id);
                    var parameters = new MatchMakerParams
                    {
                        QueueIds = new[]
                        {
                            Convert.ToInt32(config.Id)
                        }
                    };
                    Client.QueueId = config.Id;
                    Client.PVPNet.AttachToQueue(parameters, EnteredQueue);
                }
                else if (config.Id == 61)
                {
                    LobbyStatus lobby = await Client.PVPNet.createArrangedTeamLobby(config.Id);
                    Client.ClearPage(typeof (TeamBuilderPage));
                    Client.SwitchPage(new TeamBuilderPage(false, lobby));
                }
            }
            if (!InQueue)
                return;

            InQueue = false;
            Client.GameStatus = "outOfGame";
            Client.SetChatHover();
            await LeaveAllQueues();
        }

        private void EnteredQueue(SearchingForMatchNotification result)
        {
            if (result.PlayerJoinFailures != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Button item = LastSender;
                    var settings = (QueueButtonConfig)LastSender.Tag;
                    var config = (GameQueueConfig)settings.GameQueueConfig;
                    Queues.Remove(config.Id);
                    var message = new MessageOverlay
                    {
                        MessageTitle = {Content = "Failed to join queue"},
                        MessageTextBox = {Text = result.PlayerJoinFailures[0].ReasonFailed}
                    };
                    switch (result.PlayerJoinFailures[0].ReasonFailed)
                    {
                        case "QUEUE_DODGER":
                        {
                            message.MessageTextBox.Text =
                                "Unable to join the queue due to you recently dodging a game." +
                                Environment.NewLine;
                            TimeSpan time = TimeSpan.FromMilliseconds(result.PlayerJoinFailures[0].PenaltyRemainingTime);
                            message.MessageTextBox.Text = "You have " +
                                                          string.Format("{0:D2}m:{1:D2}s", time.Minutes, time.Seconds) +
                                                          " remaining until you may queue again";
                        }
                            break;
                        case "RANKED_MIN_LEVEL":
                            message.MessageTextBox.Text = "You do not meet the requirements for this queue." +
                                                          Environment.NewLine;
                            break;
                        case "QUEUE_PARTICIPANTS":
                            message.MessageTextBox.Text =
                                "This queue is in dev. Please use this queue on the real league of legends client." +
                                Environment.NewLine;
                            break;
                    }
                    Client.OverlayContainer.Content = message.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                return;
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Button item = LastSender;
                var fakeButton = new Button
                {
                    Tag = item
                }; //We require a unique button to add to the dictionary
                item.Content = "00:00";
                ButtonTimers.Add(fakeButton, 0);
            }));
            InQueue = true;
            Client.GameStatus = "inQueue";
            Client.timeStampSince =
                (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalMilliseconds;
            Client.SetChatHover();
            Client.PVPNet.OnMessageReceived += GotQueuePop;
        }

        private void GotQueuePop(object sender, object message)
        {
            if (!(message is GameDTO))
                return;

            var queue = message as GameDTO;
            if (Client.HasPopped)
                return;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Client.HasPopped = true;
                Client.OverlayContainer.Content = new QueuePopOverlay(queue, this).Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
            }));
            Client.PVPNet.OnMessageReceived -= GotQueuePop;
        }

        private void RestartDodgePop(object sender, object message)
        {
            if (message is GameDTO)
            {
                var queue = message as GameDTO;
                if (queue.GameState == "TERMINATED")
                {
                    Client.HasPopped = false;
                    Client.PVPNet.OnMessageReceived += GotQueuePop;
                }
            }
            else if (message is PlayerCredentialsDto)
            {
                Client.PVPNet.OnMessageReceived -= RestartDodgePop;
            }
        }

        internal double HighestPingTime(IPAddress[] addresses)
        {
            double[] highestPing = {-1};
            if (addresses.Length > 0)
                highestPing[0] = 0;

            foreach (PingReply reply in from address in addresses
                let timeout = 120
                let pingSender = new Ping()
                select pingSender.Send(address.ToString(), timeout)
                into reply
                where reply.Status == IPStatus.Success
                where reply.RoundtripTime > highestPing[0]
                select reply)
                highestPing[0] = reply.RoundtripTime;

            return highestPing[0];
        }

        private void CreateCustomGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInGame())
                return;

            Client.ClearPage(typeof (CreateCustomGamePage));
            Client.SwitchPage(new CreateCustomGamePage());
        }

        private void JoinFactionGameButton_Click(object sender, object e)
        {
            if (IsInGame())
                return;

            Client.ClearPage(typeof (FactionsJoinGamePage));
            Client.SwitchPage(new FactionsJoinGamePage());
        }

        private bool IsInGame()
        {
            if (!Client.IsInGame)
                return false;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var message = new MessageOverlay
                {
                    MessageTitle = {Content = "Failed to join queue"},
                    MessageTextBox =
                    {
                        Text =
                            "You are currently in a game, if you need to reconnect please return to the reconnect page above!"
                    }
                };
                Client.OverlayContainer.Content = message.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
            }));
            return true;
        }

        private void JoinCustomGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInGame())
                return;

            Client.ClearPage(typeof (CustomGameListingPage));
            Client.SwitchPage(new CustomGameListingPage());
        }

        private void AutoAcceptCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            Client.AutoAcceptQueue = (AutoAcceptCheckBox.IsChecked.HasValue) && AutoAcceptCheckBox.IsChecked.Value;
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
            Client.ClearPage(typeof (CustomGameLobbyPage));
            Client.ClearPage(typeof (CreateCustomGamePage));
            Client.ClearPage(typeof (FactionsCreateGamePage));
            Client.ClearPage(typeof (FactionsGameLobbyPage));
            Client.ClearPage(typeof (ChampSelectPage));

            foreach (Button realButton in ButtonTimers.Keys.Select(btn => (Button) btn.Tag))
                realButton.Content = "Queue (Beta)";

            ButtonTimers.Clear();
            Queues.Clear();

            return true;
        }

        private void CreateFactionGameButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsInGame())
                return;

            Client.ClearPage(typeof (FactionsCreateGamePage));
            Client.SwitchPage(new FactionsCreateGamePage());
        }
    }
}