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
using LegendaryClient.Logic.SQLite;
using System.Text.RegularExpressions;
using System.Windows.Media;

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

            var dir = new DirectoryInfo(Path.Combine(Client.ExecutingDirectory, "cabinet"));
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
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "name")))
                    item.GameId.Text = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "cabinet", d, "name"));
                else
                    item.GameId.Text = d;
                item.GameId.Tag = d;
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
                item.GameId.MouseDoubleClick += GameId_MouseDoubleClick;
                item.GameId.MouseLeave += GameId_MouseLeave;
                item.KeyDown += item_KeyDown;

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
                double k = -1, d = -1, a = -1;
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
                        default:
                            break;
                    }
                }
                foreach (object element in player.getChildElements())
                {
                    if (element is SmallChampionItem && ((SmallChampionItem)element).Name.StartsWith("game"))
                    {
                        ((SmallChampionItem)element).MouseMove += img_MouseMove;
                        ((SmallChampionItem)element).MouseLeave += img_MouseLeave;
                    }
                }

                player.ChampionIcon.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion", summary.SkinName + ".png"));
                player.File.Content = summary.SkinName;
                player.KDA.Content = k + "/" + d + "/" + a;

                TeamOnePanel.Children.Add(player);
            }

            foreach (PlayerParticipantStatsSummary summary in stats.OtherTeamPlayerParticipantStats)
            {
                double k = -1, d = -1, a = -1;
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
                        default:
                            break;
                    }
                }
                foreach (object element in player.getChildElements())
                {
                    if (element is SmallChampionItem && ((SmallChampionItem)element).Name.StartsWith("game"))
                    {
                        ((SmallChampionItem)element).MouseMove += img_MouseMove;
                        ((SmallChampionItem)element).MouseLeave += img_MouseLeave;
                    }
                }

                player.File.Content = summary.SkinName;
                player.ChampionIcon.ChampionImage.Source = Client.GetImage(Path.Combine(Client.ExecutingDirectory, "Assets", "champion", summary.SkinName + ".png"));
                player.KDA.Content = k + "/" + d + "/" + a;

                TeamTwoPanel.Children.Add(player);
            }
        }

        private void GameId_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox item = (TextBox)sender;
            if (item.IsReadOnly)
            {
                item.IsReadOnly = false;
                System.Drawing.Color c = System.Drawing.Color.White;
                item.Background = new SolidColorBrush(Color.FromRgb(c.R, c.B, c.G));
            }
        }

        private void GameId_MouseLeave(object sender, MouseEventArgs e)
        {
            TextBox item = (TextBox)sender;
            if (!item.IsReadOnly)
            {
                item.IsReadOnly = true;
                item.Background = ((Grid)item.Parent).Background;
                File.WriteAllText(Path.Combine(Client.ExecutingDirectory, "cabinet", (string)item.Tag, "name"), item.Text);
            }
        }

        void item_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                GameId_MouseLeave(((ReplayItem)sender).GameId, null);
        }

        private void img_MouseLeave(object sender, MouseEventArgs e)
        {
            SmallChampionItem icon = (SmallChampionItem)sender;
            LargeChatPlayer PlayerItem = (LargeChatPlayer)icon.Tag;

            if (PlayerItem != null)
            {
                Client.MainGrid.Children.Remove(PlayerItem);
                icon.Tag = null;
            }
        }

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            SmallChampionItem icon = (SmallChampionItem)sender;
            BitmapImage img = (BitmapImage)icon.ChampionImage.Source;
            if (img == null)
                return;

            LargeChatPlayer PlayerItem = (LargeChatPlayer)icon.Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                string[] s = img.UriSource.Segments;
                int id = int.Parse(s[s.Length - 1].Replace(".png", ""));
                Client.MainGrid.Children.Add(PlayerItem);

                items Item = items.GetItem(id);

                PlayerItem.PlayerName.Content = Item.name;

                PlayerItem.PlayerName.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                if (PlayerItem.PlayerName.DesiredSize.Width > 250) //Make title fit in item
                    PlayerItem.Width = PlayerItem.PlayerName.DesiredSize.Width;
                else
                    PlayerItem.Width = 250;

                PlayerItem.PlayerWins.Content = Item.price + " gold (" + Item.sellprice + " sell)";
                PlayerItem.PlayerLeague.Content = "Item ID " + Item.id;
                PlayerItem.LevelLabel.Content = "";
                PlayerItem.UsingLegendary.Visibility = System.Windows.Visibility.Hidden;

                string ParsedDescription = Item.description;
                ParsedDescription = ParsedDescription.Replace("<br>", Environment.NewLine);
                ParsedDescription = Regex.Replace(ParsedDescription, "<.*?>", string.Empty);
                PlayerItem.PlayerStatus.Text = ParsedDescription;

                PlayerItem.ProfileImage.Source = img;
                PlayerItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                PlayerItem.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                PlayerItem.Visibility = Visibility.Hidden;
                icon.Tag = PlayerItem;
            }

            if (PlayerItem.ActualHeight != 0 && PlayerItem.ActualWidth != 0)
            {
                double YMargin = (Client.MainGrid.ActualHeight / 2) - (PlayerItem.ActualHeight / 2);
                double XMargin = (Client.MainGrid.ActualWidth / 2) - (PlayerItem.ActualWidth / 2);
                PlayerItem.Margin = new Thickness(XMargin, YMargin, 0, 0);
                if (PlayerItem.Visibility == Visibility.Hidden)
                    PlayerItem.Visibility = Visibility.Visible;
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
                    int port = n.PlayerCredentials.ServerPort;
                    string IP;
                    if (port == 0)
                        IP = n.PlayerCredentials.ObserverServerIp + ":8088";
                    else
                        IP = n.PlayerCredentials.ObserverServerIp + ":" + port;
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
