using LegendaryClient.Elements;
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
using PVPNetConnect.RiotObjects.Platform.Statistics;
using RtmpSharp.IO;

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

        public ReplayPage()
        {
            InitializeComponent();
            Download.Visibility = Visibility.Hidden;

            if (!Directory.Exists("cabinet"))
                Directory.CreateDirectory("cabinet");

            var waitAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            waitAnimation.Completed += (o, e) =>
            {
                var showAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5));
                //ReplayGrid.BeginAnimation(Grid.OpacityProperty, showAnimation);
            };
            //ReplayGrid.BeginAnimation(Grid.OpacityProperty, waitAnimation);

            Command.TextChanged += Command_TextChanged;

            #region Register Context
            context = new SerializationContext();

            //Convert replay end of game stats to parsable object
            context.Register(typeof(EndOfGameStats));
            context.Register(typeof(PlayerParticipantStatsSummary));
            context.Register(typeof(RawStatDTO));
            #endregion Register Context

            UpdateReplays();
        }
        
        private void Username_Click(object sender, RoutedEventArgs e)
        {
            Commandname.Visibility = System.Windows.Visibility.Visible;
            Username.Visibility = System.Windows.Visibility.Hidden;
            Command.Watermark = "Enter Username Here";
            Command.Text = "Refresh";
            Command.Text = "Enter Username Here";
            Command.Watermark = "Enter Username Here";
            Download.Visibility = Visibility.Hidden;
        }

        private void Commandname_Click(object sender, RoutedEventArgs e)
        {
            Username.Visibility = System.Windows.Visibility.Visible;
            Commandname.Visibility = System.Windows.Visibility.Hidden;
            Command.Watermark = "Paste Spectator Command";
            Command.Text = "Refresh";
            Command.Text = "Paste Spectator Command";
            Command.Watermark = "Paste Spectator Command";
            Download.Visibility = Visibility.Hidden;
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

            var dir = new DirectoryInfo("cabinet");
            var directories = dir.EnumerateDirectories()
                                .OrderBy(d => d.CreationTime);

            string[] Replays = Directory.GetDirectories("cabinet");

            foreach (DirectoryInfo di in directories)
            {
                string d = di.Name;
                if (!File.Exists(Path.Combine("cabinet", d, "token")) ||
                    !File.Exists(Path.Combine("cabinet", d, "key")) ||
                    !File.Exists(Path.Combine("cabinet", d, "endOfGameStats")))
                    continue;

                byte[] Base64Stats = Convert.FromBase64String(File.ReadAllText(Path.Combine("cabinet", d, "endOfGameStats")));
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

                    Uri UriSource = new Uri("/LegendaryClient;component/Assets/champion/" + summary.SkinName + ".png", UriKind.RelativeOrAbsolute);
                    image.ChampionImage.Source = new BitmapImage(UriSource);

                    item.TeamOnePanel.Children.Add(image);
                }

                foreach (PlayerParticipantStatsSummary summary in stats.OtherTeamPlayerParticipantStats)
                {
                    SmallChampionItem image = new SmallChampionItem();
                    image.Width = 38;
                    image.Height = 38;

                    Uri UriSource = new Uri("/LegendaryClient;component/Assets/champion/" + summary.SkinName + ".png", UriKind.RelativeOrAbsolute);
                    image.ChampionImage.Source = new BitmapImage(UriSource);

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

                Uri UriSource = new Uri("/LegendaryReplays;component/champion/" + summary.SkinName + ".png", UriKind.RelativeOrAbsolute);
                player.ChampionIcon.ChampionImage.Source = new BitmapImage(UriSource);

                TeamOnePanel.Children.Add(player);
            }

            foreach (PlayerParticipantStatsSummary summary in stats.OtherTeamPlayerParticipantStats)
            {
                PlayerItemReplay player = new PlayerItemReplay();
                player.PlayerNameLabel.Content = summary.SummonerName;

                Uri UriSource = new Uri("/LegendaryReplays;component/champion/" + summary.SkinName + ".png", UriKind.RelativeOrAbsolute);
                player.ChampionIcon.ChampionImage.Source = new BitmapImage(UriSource);

                TeamTwoPanel.Children.Add(player);
            }
        }
        
        private void Download_Click(object sender, RoutedEventArgs e)
        {
            HintLabel.Content = "retrieving replay";
            HintLabel.Visibility = Visibility.Visible;
            var fadeLabelInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
            HintLabel.BeginAnimation(Label.OpacityProperty, fadeLabelInAnimation);

            string SpectatorCommand = Command.Text;
            string[] RemoveExcessInfo = SpectatorCommand.Split(new string[1] { "spectator " }, StringSplitOptions.None);

            if (RemoveExcessInfo.Length != 2)
            {
                HintLabel.Content = "invalid command";
                HintLabel.Visibility = Visibility.Visible;
                return;
            }

            string[] Info = RemoveExcessInfo[1].Replace("\"", "").Split(' ');

            if (Info.Length != 4)
            {
                HintLabel.Content = "invalid command";
                HintLabel.Visibility = Visibility.Visible;
                return;
            }

            Command.Text = "";

            int GameId = Convert.ToInt32(Info[2]);

            recorder = new ReplayRecorder(Info[0], GameId, Info[3], Info[1]);
            recorder.OnReplayRecorded += recorder_OnReplayRecorded;
            recorder.OnGotChunk += recorder_OnGotChunk;

            var fadeGridOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
            Command.Visibility = Visibility.Hidden;
            Download.Visibility = Visibility.Hidden;
        }

        void recorder_OnGotChunk(int ChunkId)
        {
            HintLabel.Visibility = Visibility.Visible;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
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

        private void SettingsButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //FindLolWindow window = new FindLolWindow();
            //window.ShowDialog();
        }

        private void WatchReplayButton_Click(object sender, RoutedEventArgs e)
        {
            string Directory = Path.Combine(@"RADS/projects/lol_game_client/releases");
            if (String.IsNullOrEmpty(Directory))
            {
                MessageBoxResult result = MessageBox.Show("You need to set your League of Legends installation location in settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ProcessStartInfo Replay = new ProcessStartInfo();
            Replay.FileName = "ReplayHandler.exe";
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

            Directory = Path.Combine(Directory, latestVersion, "deploy");

            if (!File.Exists(Path.Combine(Directory, "League of Legends.exe")))
            {
                MessageBoxResult result = MessageBox.Show("Could not find League of Legends", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return;
            }

            string[] details = selectedStats.Difficulty.Split('-');
            var p = new System.Diagnostics.Process();
            p.StartInfo.WorkingDirectory = Directory;
            p.StartInfo.FileName = Path.Combine(Directory, "League of Legends.exe");
            p.StartInfo.Arguments = "\"8393\" \"LoLLauncher.exe\" \"\" \"spectator "
                + "127.0.0.1:5651" + " "
                + File.ReadAllText(Path.Combine("cabinet", selectedStats.Difficulty, "key")) + " "
                + details[0] + " "
                + details[1] + "\"";
            p.Start();
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
