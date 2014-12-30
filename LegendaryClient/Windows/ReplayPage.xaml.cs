#region

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Replays;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using RtmpSharp.IO;
using Color = System.Drawing.Color;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ReplayPage.xaml
    /// </summary>
    public partial class ReplayPage
    {
        private readonly SerializationContext context;
        private bool User = true;
        private ReplayRecorder recorder;
        private EndOfGameStats selectedStats;

        public ReplayPage()
        {
            InitializeComponent();
            Change();

            Download.Visibility = Visibility.Hidden;

            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "cabinet"));

            var waitAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.5));
            waitAnimation.Completed +=
                (o, e) => { var showAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.5)); };

            Command.TextChanged += Command_TextChanged;

            #region Register Context

            context = new SerializationContext();

            context.Register(typeof (EndOfGameStats));
            context.Register(typeof (PlayerParticipantStatsSummary));
            context.Register(typeof (RawStatDTO));

            #endregion Register Context

            UpdateReplays();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void Command_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (Command.Text.Length == 0)
            {
                Download.Visibility = Visibility.Hidden;
                var fadeButtonOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
                fadeButtonOutAnimation.Completed += (x, y) => Download.Visibility = Visibility.Hidden;
                Download.BeginAnimation(OpacityProperty, fadeButtonOutAnimation);
            }
            else if (Command.Text.Length > 1)
                Download.Visibility = Visibility.Visible;
            else if (Command.Text.Length == 1)
            {
                Download.Visibility = Visibility.Visible;
                var fadeButtonInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
                Download.BeginAnimation(OpacityProperty, fadeButtonInAnimation);
            }
        }


        private void UpdateReplays()
        {
            GamePanel.Children.Clear();

            var dir = new DirectoryInfo(Path.Combine(Client.ExecutingDirectory, "cabinet"));
            IOrderedEnumerable<DirectoryInfo> directories = dir.EnumerateDirectories()
                .OrderBy(d => d.CreationTime);

            string[] Replays = Directory.GetDirectories(Path.Combine(Client.ExecutingDirectory, "cabinet"));

            foreach (DirectoryInfo di in directories)
            {
                string d = di.Name;
                if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "token")) ||
                    !File.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "key")) ||
                    !File.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "endOfGameStats")))
                    continue;

                byte[] base64Stats =
                    Convert.FromBase64String(
                        File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "endOfGameStats")));
                var statsReader = new AmfReader(new MemoryStream(base64Stats), context);

                var stats = (EndOfGameStats) statsReader.ReadAmf3Item();

                var item = new ReplayItem();

                //Use unoccupied variable
                stats.Difficulty = d;

                item.Tag = stats;
                item.GameId.Text = File.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "name"))
                    ? File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "name"))
                    : d;
                item.GameId.Tag = d;
                item.GameType.Content = stats.GameMode.ToLower();
                item.GameDate.Content = di.CreationTime.ToShortTimeString() + " " + di.CreationTime.ToShortDateString();
                double seconds = stats.GameLength%60;
                double minutes = stats.GameLength/60;
                item.GameTime.Content = string.Format("{0:0}:{1:00}", minutes, seconds);
                item.Margin = new Thickness(0, 5, 0, 0);

                foreach (
                    SmallChampionItem image in stats.TeamPlayerParticipantStats.Select(summary => new SmallChampionItem
                    {
                        Width = 38,
                        Height = 38,
                        ChampionImage =
                        {
                            Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion",
                                summary.SkinName + ".png"))
                        }
                    }))
                    item.TeamOnePanel.Children.Add(image);

                foreach (
                    SmallChampionItem image in
                        stats.OtherTeamPlayerParticipantStats.Select(summary => new SmallChampionItem
                        {
                            Width = 38,
                            Height = 38,
                            ChampionImage =
                            {
                                Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion",
                                    summary.SkinName + ".png"))
                            }
                        }))
                    item.TeamTwoPanel.Children.Add(image);

                item.MouseDown += item_MouseDown;
                item.GameId.MouseDoubleClick += GameId_MouseDoubleClick;
                item.GameId.MouseLeave += GameId_MouseLeave;
                item.KeyDown += item_KeyDown;

                //Insert on top
                GamePanel.Children.Insert(0, item);
            }
        }

        private void item_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = (ReplayItem) sender;
            var stats = (EndOfGameStats) item.Tag;
            selectedStats = stats;

            ReplayOverviewGrid.Visibility = Visibility.Visible;
            var fadeGridInAnimation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
            ReplayOverviewGrid.BeginAnimation(OpacityProperty, fadeGridInAnimation);

            GameId.Content = stats.Difficulty;
            GameType.Content = stats.GameMode.ToLower();
            double seconds = stats.GameLength%60;
            double minutes = stats.GameLength/60;
            GameTime.Content = string.Format("{0:0}:{1:00}", minutes, seconds);

            TeamOnePanel.Children.Clear();
            TeamTwoPanel.Children.Clear();

            foreach (PlayerParticipantStatsSummary summary in stats.TeamPlayerParticipantStats)
            {
                double k = -1, d = -1, a = -1;
                var player = new PlayerItemReplay
                {
                    PlayerNameLabel =
                    {
                        Content = summary.SummonerName
                    }
                };

                foreach (RawStatDTO stat in summary.Statistics)
                {
                    if (stat.StatTypeName.StartsWith("ITEM") && stat.Value != 0)
                    {
                        switch (stat.StatTypeName)
                        {
                            case "ITEM1":
                                player.gameItem1.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM2":
                                player.gameItem2.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM3":
                                player.gameItem3.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM4":
                                player.gameItem4.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM5":
                                player.gameItem5.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM6":
                                player.gameTrinket.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                        }
                    }
                    switch (stat.StatTypeName)
                    {
                        case "CHAMPIONS_KILLED":
                            k = stat.Value;
                            break;
                        case "NUM_DEATHS":
                            d = stat.Value;
                            break;
                        case "ASSISTS":
                            a = stat.Value;
                            break;
                    }
                }
                foreach (
                    object element in
                        player.getChildElements()
                            .Where(
                                element =>
                                    element is SmallChampionItem &&
                                    ((SmallChampionItem) element).Name.StartsWith("game")))
                {
                    ((SmallChampionItem) element).MouseMove += img_MouseMove;
                    ((SmallChampionItem) element).MouseLeave += img_MouseLeave;
                }

                player.ChampionIcon.ChampionImage.Source =
                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion",
                        summary.SkinName + ".png"));
                player.File.Content = summary.SkinName;
                player.KDA.Content = k + "/" + d + "/" + a;

                TeamOnePanel.Children.Add(player);
            }

            foreach (PlayerParticipantStatsSummary summary in stats.OtherTeamPlayerParticipantStats)
            {
                double k = -1, d = -1, a = -1;
                var player = new PlayerItemReplay
                {
                    PlayerNameLabel =
                    {
                        Content = summary.SummonerName
                    }
                };
                foreach (RawStatDTO stat in summary.Statistics)
                {
                    if (stat.StatTypeName.StartsWith("ITEM") && stat.Value != 0)
                    {
                        switch (stat.StatTypeName)
                        {
                            case "ITEM1":
                                player.gameItem1.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM2":
                                player.gameItem2.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM3":
                                player.gameItem3.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM4":
                                player.gameItem4.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM5":
                                player.gameItem5.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                            case "ITEM6":
                                player.gameTrinket.ChampionImage.Source =
                                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "item",
                                        stat.Value + ".png"));
                                break;
                        }
                    }
                    switch (stat.StatTypeName)
                    {
                        case "CHAMPIONS_KILLED":
                            k = stat.Value;
                            break;
                        case "NUM_DEATHS":
                            d = stat.Value;
                            break;
                        case "ASSISTS":
                            a = stat.Value;
                            break;
                    }
                }
                foreach (
                    object element in
                        player.getChildElements()
                            .Where(
                                element =>
                                    element is SmallChampionItem &&
                                    ((SmallChampionItem) element).Name.StartsWith("game")))
                {
                    ((SmallChampionItem) element).MouseMove += img_MouseMove;
                    ((SmallChampionItem) element).MouseLeave += img_MouseLeave;
                }

                player.File.Content = summary.SkinName;
                player.ChampionIcon.ChampionImage.Source =
                    Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion",
                        summary.SkinName + ".png"));
                player.KDA.Content = k + "/" + d + "/" + a;

                TeamTwoPanel.Children.Add(player);
            }
        }

        private void GameId_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (TextBox) sender;
            if (!item.IsReadOnly)
                return;

            item.IsReadOnly = false;
            Color c = Color.White;
            item.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(c.R, c.B, c.G));
        }

        private void GameId_MouseLeave(object sender, MouseEventArgs e)
        {
            var item = (TextBox) sender;
            if (item.IsReadOnly)
                return;

            item.IsReadOnly = true;
            item.Background = ((Grid) item.Parent).Background;
            File.WriteAllText(Path.Combine(Client.ExecutingDirectory, "cabinet", (string) item.Tag, "name"),
                item.Text);
        }

        private void item_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                GameId_MouseLeave(((ReplayItem) sender).GameId, null);
        }

        private void img_MouseLeave(object sender, MouseEventArgs e)
        {
            var icon = (SmallChampionItem) sender;
            var playerItem = (LargeChatPlayer) icon.Tag;
            if (playerItem == null)
                return;

            Client.MainGrid.Children.Remove(playerItem);
            icon.Tag = null;
        }

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            var icon = (SmallChampionItem) sender;
            var img = (BitmapImage) icon.ChampionImage.Source;
            if (img == null)
                return;

            var playerItem = (LargeChatPlayer) icon.Tag;
            if (playerItem == null)
            {
                string[] s = img.UriSource.Segments;
                int id = int.Parse(s[s.Length - 1].Replace(".png", ""));
                Client.MainGrid.Children.Add(playerItem);

                items item = items.GetItem(id);

                playerItem = new LargeChatPlayer();

                playerItem.PlayerName.Content = item.name;
                playerItem.PlayerName.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                playerItem.Width = playerItem.PlayerName.DesiredSize.Width > 250
                    ? playerItem.PlayerName.DesiredSize.Width
                    : 250;
                playerItem.PlayerWins.Content = item.price + " gold (" + item.sellprice + " sell)";
                playerItem.PlayerLeague.Content = "Item ID " + item.id;
                playerItem.LevelLabel.Content = "";
                playerItem.UsingLegendary.Visibility = Visibility.Hidden;

                string parsedDescription = item.description;
                parsedDescription = parsedDescription.Replace("<br>", Environment.NewLine);
                parsedDescription = Regex.Replace(parsedDescription, "<.*?>", string.Empty);
                playerItem.PlayerStatus.Text = parsedDescription;

                playerItem.ProfileImage.Source = img;
                playerItem.HorizontalAlignment = HorizontalAlignment.Left;
                playerItem.VerticalAlignment = VerticalAlignment.Top;
                playerItem.Visibility = Visibility.Hidden;
                icon.Tag = playerItem;
            }

            if (playerItem.ActualHeight == 0 || playerItem.ActualWidth == 0)
                return;

            double yMargin = (Client.MainGrid.ActualHeight/2) - (playerItem.ActualHeight/2);
            double xMargin = (Client.MainGrid.ActualWidth/2) - (playerItem.ActualWidth/2);
            playerItem.Margin = new Thickness(xMargin, yMargin, 0, 0);
            if (playerItem.Visibility == Visibility.Hidden)
                playerItem.Visibility = Visibility.Visible;
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            if (User)
            {
                PublicSummoner summoner = await Client.PVPNet.GetSummonerByName(Command.Text);
                if (String.IsNullOrWhiteSpace(summoner.Name))
                {
                    var overlay = new MessageOverlay
                    {
                        MessageTitle = {Content = "No Summoner Found"},
                        MessageTextBox = {Text = "The summoner \"" + Command.Text + "\" does not exist."}
                    };
                    Client.OverlayContainer.Content = overlay.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;

                    return;
                }
                HintLabel.Content = "retrieving replay";
                HintLabel.Visibility = Visibility.Visible;
                var fadeLabelInAnimationx = new DoubleAnimation(1, TimeSpan.FromSeconds(0.1));
                HintLabel.BeginAnimation(OpacityProperty, fadeLabelInAnimationx);
                PlatformGameLifecycleDTO n = await Client.PVPNet.RetrieveInProgressSpectatorGameInfo(Command.Text);
                if (n.GameName != null)
                {
                    string ip = n.PlayerCredentials.ObserverServerIp + ":" + n.PlayerCredentials.ObserverServerPort;
                    string key = n.PlayerCredentials.ObserverEncryptionKey;
                    var gameId = (Int32) n.PlayerCredentials.GameId;
                    recorder = new ReplayRecorder(ip, gameId, Client.Region.InternalName, key);
                    recorder.OnReplayRecorded += recorder_OnReplayRecorded;
                    recorder.OnGotChunk += recorder_OnGotChunk;

                    var fadeGridOutAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.1));
                    Command.Visibility = Visibility.Hidden;
                    Download.Visibility = Visibility.Hidden;
                    HintLabel.Visibility = Visibility.Visible;
                    HintLabel.Content = "Starting replay download";
                }
                HintLabel.Content = "That player is not in a game";
                HintLabel.Visibility = Visibility.Visible;
            }
        }

        private void recorder_OnGotChunk(int chunkId)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                HintLabel.Visibility = Visibility.Visible;
                HintLabel.Content = "retrieving replay (got chunk " + chunkId + ")";
            }));
        }

        private void recorder_OnReplayRecorded()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                HintLabel.Margin = new Thickness(0, 30, 365, 0);
                HintLabel.Content = "Replay Downloaded";
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
            string directory = Client.Location;
            if (String.IsNullOrEmpty(directory))
            {
                MessageBoxResult result =
                    MessageBox.Show("You need to set your League of Legends installation location in settings.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var Replay = new ProcessStartInfo();
            Replay.FileName = Path.Combine(Client.ExecutingDirectory, "ReplayHandler.exe");
            Replay.Verb = "runas";
            Replay.Arguments = selectedStats.Difficulty.Replace('-', ' ');
            Process.Start(Replay);

            //string Directory = Path.Combine(@"RADS/projects/lol_game_client/releases");

            var dInfo = new DirectoryInfo(directory);
            DirectoryInfo[] subdirs = null;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch
            {
                MessageBoxResult result = MessageBox.Show("Could not find League of Legends", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            directory = Path.Combine(Client.Location);

            if (!File.Exists(Path.Combine(directory, "League of Legends.exe")))
            {
                MessageBoxResult result = MessageBox.Show("Could not find League of Legends", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            string[] details = selectedStats.Difficulty.Split('-');
            Client.LaunchSpectatorGame("127.0.0.1:5651",
                File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "cabinet", selectedStats.Difficulty, "key")),
                Convert.ToInt32(details[0]), details[1]);
        }
        private void deleteReplayButton_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.Combine(Client.ExecutingDirectory, "cabinet", selectedStats.Difficulty);
            if (Directory.Exists(path))
                Directory.Delete(path,true);
            UpdateReplays();
            ReplayOverviewGrid.Visibility = Visibility.Hidden;
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateReplays();
        }
    }

    public class FocusVisualTreeChanger
    {
        public static readonly DependencyProperty IsChangedProperty =
            DependencyProperty.RegisterAttached("IsChanged", typeof (bool), typeof (FocusVisualTreeChanger),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, IsChangedCallback));

        public static bool GetIsChanged(DependencyObject obj)
        {
            return (bool) obj.GetValue(IsChangedProperty);
        }

        public static void SetIsChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(IsChangedProperty, value);
        }

        private static void IsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!true.Equals(e.NewValue))
                return;

            var contentElement = d as FrameworkContentElement;
            if (contentElement != null)
            {
                contentElement.FocusVisualStyle = null;

                return;
            }

            var element = d as FrameworkElement;
            if (element != null)
                element.FocusVisualStyle = null;
        }
    }
}