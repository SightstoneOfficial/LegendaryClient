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
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using Timer = System.Timers.Timer;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for PlayPage.xaml
    /// </summary>
    public partial class PlayPage : Page
    {
        static Timer PingTimer;
        Dictionary<double, JoinQueue> configs = new Dictionary<double, JoinQueue>();

        public PlayPage()
        {
            InitializeComponent();
            Client.IsOnPlayPage = true;
            PingTimer = new Timer(10000);
            PingTimer.Elapsed += new ElapsedEventHandler(PingElapsed);
            PingTimer.Enabled = true;
            PingElapsed(1, null);
        }

        internal void PingElapsed(object sender, ElapsedEventArgs e)
        {
            if (!Client.IsOnPlayPage)
                return;
            double PingAverage = HighestPingTime(Client.Region.PingAddresses);
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
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
                    item.QueueLabel.Content = InternalQueueToPretty(config.CacheName);
                    QueueInfo t = await Client.PVPNet.GetQueueInformation(config.Id);
                    item.AmountInQueueLabel.Content = "People in queue: " + t.QueueLength;
                    TimeSpan time = TimeSpan.FromMilliseconds(t.WaitTime);
                    string answer = string.Format("{0:D2}m:{1:D2}s", time.Minutes, time.Seconds);
                    item.WaitTimeLabel.Content = "Wait Time: " + answer;
                    if (!configs.ContainsKey(config.Id))
                    {
                        configs.Add(config.Id, item);
                        QueueListView.Items.Add(item);
                    }
                }
            }));
        }

        internal string InternalQueueToPretty(string InternalQueue)
        {
            switch (InternalQueue)
            {
                case "matching-queue-NORMAL-5x5-game-queue":
                    return "Normal 5v5";
                case "matching-queue-NORMAL-3x3-game-queue":
                    return "Normal 3v3";
                case "matching-queue-NORMAL-5x5-draft-game-queue":
                    return "Draft 5v5";
                case "matching-queue-RANKED_SOLO-5x5-game-queue":
                    return "Ranked 5v5";
                case "matching-queue-RANKED_TEAM-3x3-game-queue":
                    return "Ranked Team 5v5";
                case "matching-queue-RANKED_TEAM-5x5-game-queue":
                    return "Ranked Team 3v3";
                case "matching-queue-ODIN-5x5-game-queue":
                    return "Dominion 5v5";
                case "matching-queue-ARAM-5x5-game-queue":
                    return "ARAM 5v5";
                case "matching-queue-BOT-5x5-game-queue":
                    return "Bot 5v5 Beginner";
                case "matching-queue-ODIN-5x5-draft-game-queue":
                    return "Dominion Draft 5v5";
                case "matching-queue-BOT_TT-3x3-game-queue":
                    return "Bot 3v3 Beginner";
                case "matching-queue-ODINBOT-5x5-game-queue":
                    return "Dominion Bot 5v5 Beginner";
                default:
                    return InternalQueue;
            }
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
    }
}
