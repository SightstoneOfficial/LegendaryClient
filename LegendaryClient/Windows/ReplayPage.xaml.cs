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
using LegendaryClient.Logic.Replays;
using System.Windows.Threading;
using System.IO;
//using System.IO.Path;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using LegendaryReplays;
using RtmpSharp.IO;
using LegendaryReplays.Elements;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ReplayPage.xaml
    /// </summary>
    public partial class ReplayPage : Page
    {
        ReplayRecorder recorder;
        public ReplayPage()
        {
            InitializeComponent();
            Download.Visibility = Visibility.Hidden;
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            //HintLabel.Margin = new Thickness(0, 30, 365, 0);
            HintLabel.Content = "retrieving replay";
            //var fadeLabelInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
            //HintLabel.BeginAnimation(Label.OpacityProperty, fadeLabelInAnimation);

            string SpectatorCommand = Command.Text;
            string[] RemoveExcessInfo = SpectatorCommand.Split(new string[1] { "spectator " }, StringSplitOptions.None);

            if (RemoveExcessInfo.Length != 2)
            {
                HintLabel.Visibility = Visibility.Visible;
                HintLabel.Content = "invalid command";
                //HintLabel.Margin = new Thickness(0, 60, 365, 0);
                return;
            }

            string[] Info = RemoveExcessInfo[1].Replace("\"", "").Split(' ');

            if (Info.Length != 4)
            {
                HintLabel.Visibility = Visibility.Visible;
                HintLabel.Content = "invalid command";
                //HintLabel.Margin = new Thickness(0, 60, 150, 0);
                return;
            }

            Command.Text = "";

            int GameId = Convert.ToInt32(Info[2]);

            recorder = new ReplayRecorder(Info[0], GameId, Info[3], Info[1]);
            recorder.OnReplayRecorded += recorder_OnReplayRecorded;
            recorder.OnGotChunk += recorder_OnGotChunk;

            //var fadeGridOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
            //InputGrid.BeginAnimation(Grid.OpacityProperty, fadeGridOutAnimation);
            //InputGrid.Visibility = Visibility.Hidden;
        }
        void recorder_OnGotChunk(int ChunkId)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                HintLabel.Content = "retrieving replay (got chunk " + ChunkId + ")";
            }));
        }

        void recorder_OnReplayRecorded()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                HintLabel.Visibility = Visibility.Hidden;
                //HintLabel.Margin = new Thickness(0, 30, 365, 0);
                Command.Watermark = "replay downloaded";
                //var fadeGridInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
                //InputGrid.BeginAnimation(Grid.OpacityProperty, fadeGridInAnimation);
                //InputGrid.Visibility = Visibility.Visible;
                //UpdateReplays();
            }));
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
            HintLabel.Visibility = Visibility.Hidden;
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
            HintLabel.Visibility = Visibility.Hidden;
        }

        void Command_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Command.Text.Length == 0)
            {
                Download.Visibility = Visibility.Hidden;
                HintLabel.Visibility = Visibility.Hidden;
                //var fadeButtonOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
                //fadeButtonOutAnimation.Completed += (x, y) => Download.Visibility = Visibility.Hidden;
                //Download.BeginAnimation(Button.OpacityProperty, fadeButtonOutAnimation);
            }
            
            else if (Command.Text.Length > 1)
            {
                Download.Visibility = Visibility.Visible;
                HintLabel.Visibility = Visibility.Hidden;
            }
            else if (Command.Text.Length == 1)
            {

                Download.Visibility = Visibility.Visible;
                HintLabel.Visibility = Visibility.Hidden;
                //var fadeButtonInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
                //Download.BeginAnimation(Button.OpacityProperty, fadeButtonInAnimation);
            }
        }


        void item_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ReplayItem item = (ReplayItem)sender;
            EndOfGameStats stats = (EndOfGameStats)item.Tag;
            selectedStats = stats;

            //ReplayOverviewGrid.Visibility = Visibility.Visible;
            //var fadeGridInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
            //ReplayOverviewGrid.BeginAnimation(Grid.OpacityProperty, fadeGridInAnimation);

            GameId.Content = stats.Difficulty;
            GameType.Content = stats.GameMode.ToLower();
            double seconds = stats.GameLength % 60;
            double minutes = stats.GameLength / 60;
            GameTime.Content = string.Format("{0:0}:{1:00}", minutes, seconds);

            TeamOnePanel.Children.Clear();
            TeamTwoPanel.Children.Clear();

            foreach (PlayerParticipantStatsSummary summary in stats.TeamPlayerParticipantStats)
            {
                PlayerItem player = new PlayerItem();
                player.PlayerNameLabel.Content = summary.SummonerName;

                Uri UriSource = new Uri("/LegendaryReplays;component/champion/" + summary.SkinName + ".png", UriKind.RelativeOrAbsolute);
                player.ChampionIcon.ChampionImage.Source = new BitmapImage(UriSource);

                TeamOnePanel.Children.Add(player);
            }

            foreach (PlayerParticipantStatsSummary summary in stats.OtherTeamPlayerParticipantStats)
            {
                PlayerItem player = new PlayerItem();
                player.PlayerNameLabel.Content = summary.SummonerName;

                Uri UriSource = new Uri("/LegendaryReplays;component/champion/" + summary.SkinName + ".png", UriKind.RelativeOrAbsolute);
                player.ChampionIcon.ChampionImage.Source = new BitmapImage(UriSource);

                TeamTwoPanel.Children.Add(player);
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
                if (!File.Exists(System.IO.Path.Combine("cabinet", d, "token")) ||
                    !File.Exists(System.IO.Path.Combine("cabinet", d, "key")) ||
                    !File.Exists(System.IO.Path.Combine("cabinet", d, "endOfGameStats")))
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

                    Uri UriSource = new Uri("/LegendaryClient;component/champion/" + summary.SkinName + ".png", UriKind.RelativeOrAbsolute);
                    image.ChampionImage.Source = new BitmapImage(UriSource);

                    item.TeamOnePanel.Children.Add(image);
                }

                foreach (PlayerParticipantStatsSummary summary in stats.OtherTeamPlayerParticipantStats)
                {
                    SmallChampionItem image = new SmallChampionItem();
                    image.Width = 38;
                    image.Height = 38;

                    Uri UriSource = new Uri("/LegendaryClient;component/champion/" + summary.SkinName + ".png", UriKind.RelativeOrAbsolute);
                    image.ChampionImage.Source = new BitmapImage(UriSource);

                    item.TeamTwoPanel.Children.Add(image);
                }

                item.MouseDown += item_MouseDown;

                //Insert on top
                GamePanel.Children.Insert(0, item);
            }
        }
    }
}
