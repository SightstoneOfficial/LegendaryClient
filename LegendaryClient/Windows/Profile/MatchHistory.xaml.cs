using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for MatchHistory.xaml
    /// </summary>
    public partial class MatchHistory : Page
    {
        private List<PlayerGameStats> GameStats = new List<PlayerGameStats>();
        private LargeChatPlayer PlayerItem;

        public MatchHistory()
        {
            InitializeComponent();
        }

        public void Update(double AccountId)
        {
            Client.PVPNet.GetRecentGames(AccountId, new RecentGames.Callback(GotRecentGames));
        }

        public void GotRecentGames(RecentGames result)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                GamesListView.Items.Clear();
                BlueListView.Items.Clear();
                ItemsListView.Items.Clear();
                PurpleListView.Items.Clear();
                GameStatsListView.Items.Clear();
                GameStats = result.GameStatistics;
                GameStats.Sort((s1, s2) => s2.CreateDate.CompareTo(s1.CreateDate));
                foreach (PlayerGameStats Game in GameStats)
                {
                    RecentGameOverview item = new RecentGameOverview();
                    item.ChampionImage.Source = champions.GetChampion((int)Math.Round(Game.ChampionId)).icon;
                    bool Lose = false;
                    double Deaths = 0;
                    double Kills = 0;
                    double Assists = 0;
                    double CS = 0;
                    foreach (RawStat Stat in Game.Statistics)
                    {
                        if (Stat.StatType == "LOSE")
                            if (Stat.Value == 1)
                                Lose = true;

                        if (Stat.StatType == "NUM_DEATHS")
                            Deaths = Stat.Value;

                        if (Stat.StatType == "ASSISTS")
                            Assists = Stat.Value;

                        if (Stat.StatType == "CHAMPIONS_KILLED")
                            Kills = Stat.Value;

                        if (Stat.StatType == "MINIONS_KILLED")
                            CS += Stat.Value;

                        if (Stat.StatType == "NEUTRAL_MINIONS_KILLED")
                            CS += Stat.Value;
                    }
                    item.ChampionNameLabel.Content = champions.GetChampion(Convert.ToInt32(Game.ChampionId)).displayName;
                    item.ScoreLabel.Content = Kills + "/" + Deaths + "/" + Assists + "(" + TitleCaseString(Game.GameType.Replace("_GAME", "")
                        .Replace("MATCHED", "NORMAL")
                        .Replace('_', ' ')) + ")";
                    item.CreepScoreLabel.Content = CS + " minions";
                    item.DateLabel.Content = Game.CreateDate;
                    item.IPEarnedLabel.Content = "+" + Game.IpEarned + " IP";
                    item.PingLabel.Content = Game.UserServerPing + "ms";
                    if (Lose)
                    {
                        BrushConverter bc = new BrushConverter();
                        Brush brush = (Brush)bc.ConvertFrom("#FF9E6060");
                        item.GridView.Background = brush;
                    }
                    else
                    {
                        BrushConverter bc = new BrushConverter();
                        Brush brush = (Brush)bc.ConvertFrom("#FF609E74");
                        item.GridView.Background = brush;
                    }
                    item.GridView.Width = 250;
                    GamesListView.Items.Add(item);
                }
            }));
        }

        private async void GamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GamesListView.SelectedIndex != -1)
            {
                PlayerGameStats stats = GameStats[GamesListView.SelectedIndex];

                Dictionary<double, string> IDtoName = new Dictionary<double, string>();
                List<double> SummonerIDs = new List<double>();
                foreach (FellowPlayerInfo info in stats.FellowPlayers)
                {
                    SummonerIDs.Add(info.SummonerId);
                }
                string[] SummonerNames = await Client.PVPNet.GetSummonerNames(SummonerIDs.ToArray());
                for (int i = 0; i < SummonerNames.Length; i++)
                {
                    IDtoName.Add(SummonerIDs[i], SummonerNames[i]);
                }
                GameStatsListView.Items.Clear();
                PurpleListView.Items.Clear();
                BlueListView.Items.Clear();
                ItemsListView.Items.Clear();

                //Add self to game players
                Image img = new Image();
                img.Width = 58;
                img.Height = 58;
                img.Source = champions.GetChampion((int)Math.Round(stats.ChampionId)).icon;
                BlueListView.Items.Add(img);

                foreach (FellowPlayerInfo info in stats.FellowPlayers)
                {
                    if (info.TeamId == stats.TeamId)
                    {
                        Image i = new Image();
                        i.Width = 58;
                        i.Height = 58;
                        i.Source = champions.GetChampion((int)Math.Round(info.ChampionId)).icon;
                        BlueListView.Items.Add(i);
                    }
                    else
                    {
                        Image i = new Image();
                        i.Width = 58;
                        i.Height = 58;
                        i.Source = champions.GetChampion((int)Math.Round(info.ChampionId)).icon;
                        PurpleListView.Items.Add(i);
                    }
                }
                foreach (RawStat Stat in stats.Statistics)
                {
                    if (!Stat.StatType.StartsWith("ITEM"))
                    {
                        ProfilePage.KeyValueItem item = new ProfilePage.KeyValueItem
                        {
                            Key = TitleCaseString(Stat.StatType.Replace('_', ' ')),
                            Value = Stat.Value.ToString()
                        };
                        GameStatsListView.Items.Add(item);
                    }
                    else
                    {
                        try
                        {
                            ProfilePage.KeyValueItem item = new ProfilePage.KeyValueItem
                            {
                                Key = TitleCaseString(Stat.StatType.Replace('_', ' ')),
                                Value = Stat.Value.ToString()
                            };
                            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "item", item.Value + ".png"), UriKind.Absolute);
                            img = new Image();
                            img.Width = 58;
                            img.Height = 58;
                            img.Source = new BitmapImage(uriSource);
                            img.Tag = item;
                            img.MouseMove += img_MouseMove;
                            img.MouseLeave += img_MouseLeave;
                            ItemsListView.Items.Add(img);
                        }
                        catch { }
                    }
                }

                //Resize Columns
                if (double.IsNaN(GameKeyHeader.Width))
                    GameKeyHeader.Width = GameKeyHeader.ActualWidth;
                if (double.IsNaN(GameValueHeader.Width))
                    GameValueHeader.Width = GameValueHeader.ActualWidth;
                GameKeyHeader.Width = double.NaN;
                GameValueHeader.Width = double.NaN;
            }
        }

        private void img_MouseLeave(object sender, MouseEventArgs e)
        {
            if (PlayerItem != null)
            {
                Client.MainGrid.Children.Remove(PlayerItem);
                PlayerItem = null;
            }
        }

        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            Image item = (Image)sender;
            ProfilePage.KeyValueItem playerItem = (ProfilePage.KeyValueItem)item.Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(PlayerItem);
            }
            PlayerItem.Tag = playerItem;

            items Item = items.GetItem(Convert.ToInt32(playerItem.Value));

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

            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "item", Item.id + ".png"), UriKind.RelativeOrAbsolute);
            PlayerItem.ProfileImage.Source = new BitmapImage(uriSource);

            PlayerItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            PlayerItem.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            PlayerItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Point MouseLocation = e.GetPosition(Client.MainGrid);

            double YMargin = MouseLocation.Y;

            double XMargin = MouseLocation.X;
            if (XMargin + PlayerItem.Width + 10 > Client.MainGrid.ActualWidth)
                XMargin = Client.MainGrid.ActualWidth - PlayerItem.Width - 10;

            PlayerItem.Margin = new Thickness(XMargin + 5, YMargin + 5, 0, 0);
        }

        public static String TitleCaseString(String s)
        {
            if (s == null) return s;

            String[] words = s.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0) continue;

                Char firstChar = Char.ToUpper(words[i][0]);
                String rest = "";
                if (words[i].Length > 1)
                {
                    rest = words[i].Substring(1).ToLower();
                }
                words[i] = firstChar + rest;
            }
            return String.Join(" ", words);
        }
    }
}