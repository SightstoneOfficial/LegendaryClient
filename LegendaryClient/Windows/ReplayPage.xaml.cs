using LegendaryClient.Controls;
using LegendaryClient.Properties;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LegendaryClient.Logic.Replays;
using RtmpSharp.IO;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Summoner;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ReplayPage.xaml
    /// </summary>
    public partial class ReplayPage : Page
    {
        ReplayRecorder recorder;
        SerializationContext context;
        EndOfGameStats selectedStats;
        bool User = true;

        public ReplayPage()
        {
            InitializeComponent();
            Download.Visibility = Visibility.Hidden;

            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "cabinet"));

            var waitAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            waitAnimation.Completed += (o, e) =>
            {
                var showAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
            };

            Command.TextChanged += Command_TextChanged;

            #region Register Context
            context = new SerializationContext();

            context.Register(typeof(EndOfGameStats));
            context.Register(typeof(PlayerParticipantStatsSummary));
            context.Register(typeof(RawStatDTO));
            #endregion Register Context

            UpdateReplays();
        }

        void Command_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Command.Text.Length == 0)
            {
                Download.Visibility = Visibility.Hidden;
                var fadeButtonOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
                fadeButtonOutAnimation.Completed += (x, y) => Download.Visibility = Visibility.Hidden;
                Download.BeginAnimation(Button.OpacityProperty, fadeButtonOutAnimation);
            }
            
            else if (Command.Text.Length > 1)
            {
                Download.Visibility = Visibility.Visible;
            }
            else if (Command.Text.Length == 1)
            {

                Download.Visibility = Visibility.Visible;
                var fadeButtonInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
                Download.BeginAnimation(Button.OpacityProperty, fadeButtonInAnimation);
            }
        }


        void UpdateReplays()
        {
            GamePanel.Children.Clear();

            var dir = new DirectoryInfo(Path.Combine(Client.ExecutingDirectory,"cabinet"));
            var directories = dir.EnumerateDirectories()
                                .OrderBy(d => d.CreationTime);

            string[] Replays = Directory.GetDirectories(Path.Combine(Client.ExecutingDirectory, "cabinet"));

            foreach (DirectoryInfo di in directories)
            {
                string d = di.Name;
                if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "token")) ||
                    !File.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "key")) ||
                    !File.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "endOfGameStats")))
                    continue;

                byte[] Base64Stats = Convert.FromBase64String(File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "endOfGameStats")));
                AmfReader statsReader = new AmfReader(new MemoryStream(Base64Stats), context);

                EndOfGameStats stats = (EndOfGameStats)statsReader.ReadAmf3Item();

                ReplayItem item = new ReplayItem();

                //Use unoccupied variable
                stats.Difficulty = d;

                item.Tag = stats;
                item.GameId.Content = d;
                item.GameType.Content = stats.GameMode.ToLower();
                item.GameDate.Content = di.CreationTime.ToShortTimeString() + " " + di.CreationTime.ToShortDateString();
                double seconds = stats.GameLength % 60;
                double minutes = stats.GameLength / 60;
                item.GameTime.Content = string.Format("{0:0}:{1:00}", minutes, seconds);
                item.Margin = new Thickness(0, 5, 0, 0);

                foreach (PlayerParticipantStatsSummary summary in stats.TeamPlayerParticipantStats)
                {
                    SmallChampionItem image = new SmallChampionItem();
                    image.Width = 38;
                    image.Height = 38;

                    image.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion", summary.SkinName + ".png"));

                    item.TeamOnePanel.Children.Add(image);
                }

                foreach (PlayerParticipantStatsSummary summary in stats.OtherTeamPlayerParticipantStats)
                {
                    SmallChampionItem image = new SmallChampionItem();
                    image.Width = 38;
                    image.Height = 38;

                    image.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion", summary.SkinName + ".png"));

                    item.TeamTwoPanel.Children.Add(image);
                }

                item.MouseDown += item_MouseDown;

                //Insert on top
                GamePanel.Children.Insert(0, item);
            }
        }

        void item_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ReplayItem item = (ReplayItem)sender;
            EndOfGameStats stats = (EndOfGameStats)item.Tag;
            selectedStats = stats;

            ReplayOverviewGrid.Visibility = Visibility.Visible;
            var fadeGridInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
            ReplayOverviewGrid.BeginAnimation(Grid.OpacityProperty, fadeGridInAnimation);

            GameId.Content = stats.Difficulty;
            GameType.Content = stats.GameMode.ToLower();
            double seconds = stats.GameLength % 60;
            double minutes = stats.GameLength / 60;
            GameTime.Content = string.Format("{0:0}:{1:00}", minutes, seconds);

            TeamOnePanel.Children.Clear();
            TeamTwoPanel.Children.Clear();

            foreach (PlayerParticipantStatsSummary summary in stats.TeamPlayerParticipantStats)
            {
                PlayerItemReplay player = new PlayerItemReplay();
                player.PlayerNameLabel.Content = summary.SummonerName;
                foreach (RawStatDTO stat in summary.Statistics)
                {
                    if (stat.StatTypeName.StartsWith("ITEM") && stat.Value != 0)
                    {
                        switch (stat.StatTypeName)
                        {
                            case "ITEM1":
                                player.gameItem1.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            case "ITEM2":
                                player.gameItem2.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            case "ITEM3":
                                player.gameItem3.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png")); 
                                break;
                            case "ITEM4":
                                player.gameItem4.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            case "ITEM5":
                                player.gameItem5.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            case "ITEM6":
                                player.gameTrinket.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            default:
                                break;
                        }
                    }
                }
                player.ChampionIcon.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion", summary.SkinName + ".png"));
                player.File.Content = summary.SkinName;

                TeamOnePanel.Children.Add(player);
            }

            foreach (PlayerParticipantStatsSummary summary in stats.OtherTeamPlayerParticipantStats)
            {
                PlayerItemReplay player = new PlayerItemReplay();
                player.PlayerNameLabel.Content = summary.SummonerName;
                foreach (RawStatDTO stat in summary.Statistics)
                {
                    if (stat.StatTypeName.StartsWith("ITEM") && stat.Value != 0)
                    {
                        switch (stat.StatTypeName)
                        {
                            case "ITEM1":
                                player.gameItem1.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            case "ITEM2":
                                player.gameItem2.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            case "ITEM3":
                                player.gameItem3.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            case "ITEM4":
                                player.gameItem4.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            case "ITEM5":
                                player.gameItem5.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            case "ITEM6":
                                player.gameTrinket.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item", stat.Value + ".png"));
                                break;
                            default:
                                break;
                        }
                    }
                }
                player.File.Content = summary.SkinName;
                player.ChampionIcon.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion", summary.SkinName + ".png"));

                TeamTwoPanel.Children.Add(player);
            }
        }
        
        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (User == true)
            {
                PublicSummoner Summoner = await Client.PVPNet.GetSummonerByName(Command.Text);
                if (String.IsNullOrWhiteSpace(Summoner.Name))
                {
                    MessageOverlay overlay = new MessageOverlay();
                    overlay.MessageTitle.Content = "No Summoner Found";
                    overlay.MessageTextBox.Text = "The summoner \"" + Command.Text + "\" does not exist.";
                    Client.OverlayContainer.Content = overlay.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                    return;
                }
                HintLabel.Content = "retrieving replay";
                HintLabel.Visibility = Visibility.Visible;
                var fadeLabelInAnimationx = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
                HintLabel.BeginAnimation(Label.OpacityProperty, fadeLabelInAnimationx);
                PlatformGameLifecycleDTO n = await Client.PVPNet.RetrieveInProgressSpectatorGameInfo(Command.Text);
                if (n.GameName != null)
                {
                    string IP = n.PlayerCredentials.ObserverServerIp;
                    string Key = n.PlayerCredentials.ObserverEncryptionKey;
                    int GameID = (Int32)n.PlayerCredentials.GameId;
                    recorder = new ReplayRecorder(IP, GameID, Client.Region.InternalName, Key);
                    recorder.OnReplayRecorded += recorder_OnReplayRecorded;
                    recorder.OnGotChunk += recorder_OnGotChunk;

                    var fadeGridOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
                    Command.Visibility = Visibility.Hidden;
                    Download.Visibility = Visibility.Hidden;
                    HintLabel.Visibility = Visibility.Visible;
                    HintLabel.Content = "Starting replay download";
                    return;
                }
                else
                {
                    HintLabel.Content = "That player is not in a game";
                    HintLabel.Visibility = Visibility.Visible;
                    return;
                }
            }
        }

        void recorder_OnGotChunk(int ChunkId)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                HintLabel.Visibility = Visibility.Visible;
                HintLabel.Content = "retrieving replay (got chunk " + ChunkId + ")";
            }));
        }

        void recorder_OnReplayRecorded()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                HintLabel.Margin = new Thickness(0, 30, 365, 0);
                Command.Watermark = "replay downloaded";
                var fadeGridInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
                Command.Visibility = Visibility.Visible;
                Download.Visibility = Visibility.Visible;
                UpdateReplays();
            }));
        }

        private void ExitButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void WatchReplayButton_Click(object sender, RoutedEventArgs e)
        {
            string Directory = Client.Location;
            if (String.IsNullOrEmpty(Directory))
            {
                MessageBoxResult result = MessageBox.Show("You need to set your League of Legends installation location in settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ProcessStartInfo Replay = new ProcessStartInfo();
            Replay.FileName = Path.Combine(Client.ExecutingDirectory, "ReplayHandler.exe");
            Replay.Verb = "runas";
            Replay.Arguments = selectedStats.Difficulty.Replace('-', ' ');
            Process.Start(Replay);

            //string Directory = Path.Combine(@"RADS/projects/lol_game_client/releases");

            DirectoryInfo dInfo = new DirectoryInfo(Directory);
            DirectoryInfo[] subdirs = null;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch { MessageBoxResult result = MessageBox.Show("Could not find League of Legends", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
            {
                latestVersion = info.Name;
            }

            Directory = Path.Combine(Client.Location);

            if (!File.Exists(Path.Combine(Directory, "League of Legends.exe")))
            {
                MessageBoxResult result = MessageBox.Show("Could not find League of Legends", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return;
            }

            string[] details = selectedStats.Difficulty.Split('-');
            Client.LaunchSpectatorGame("127.0.0.1:5651", File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "cabinet", selectedStats.Difficulty, "key")), Convert.ToInt32(details[0]), details[1]);
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateReplays();
        }
    }

    public class FocusVisualTreeChanger
    {
        public static bool GetIsChanged(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsChangedProperty);
        }

        public static void SetIsChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(IsChangedProperty, value);
        }

        public static readonly DependencyProperty IsChangedProperty =
            DependencyProperty.RegisterAttached("IsChanged", typeof(bool), typeof(FocusVisualTreeChanger), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, IsChangedCallback));

        private static void IsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (true.Equals(e.NewValue))
            {
                FrameworkContentElement contentElement = d as FrameworkContentElement;
                if (contentElement != null)
                {
                    contentElement.FocusVisualStyle = null;
                    return;
                }

                FrameworkElement element = d as FrameworkElement;
                if (element != null)
                {
                    element.FocusVisualStyle = null;
                }
            }
        }
    }
}
