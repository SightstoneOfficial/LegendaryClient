using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using log4net;
using PVPNetConnect.RiotObjects.Platform.Statistics;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for MatchHistory.xaml
    /// </summary>
    public partial class MatchHistory
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (MatchHistory));
        private readonly List<MatchStats> GameStats = new List<MatchStats>();
        private LargeChatPlayer PlayerItem;

        public MatchHistory()
        {
            InitializeComponent();
        }

        public void Update(double AccountId)
        {
            Client.PVPNet.GetRecentGames(AccountId, GotRecentGames);
        }

        public void GotRecentGames(RecentGames result)
        {
            GameStats.Clear();
            result.GameStatistics.Sort((s1, s2) => s2.CreateDate.CompareTo(s1.CreateDate));
            foreach (PlayerGameStats Game in result.GameStatistics)
            {
                Game.GameType = Client.TitleCaseString(Game.GameType.Replace("_GAME", "").Replace("MATCHED", "NORMAL"));
                var Match = new MatchStats();

                foreach (RawStat Stat in Game.Statistics)
                {
                    Type type = typeof (MatchStats);
                    string fieldName = Client.TitleCaseString(Stat.StatType.Replace('_', ' ')).Replace(" ", "");
                    FieldInfo f = type.GetField(fieldName);
                    f.SetValue(Match, Stat.Value);
                }

                Match.Game = Game;

                GameStats.Add(Match);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                GamesListView.Items.Clear();
                BlueListView.Items.Clear();
                ItemsListView.Items.Clear();
                PurpleListView.Items.Clear();
                GameStatsListView.Items.Clear();
                foreach (MatchStats stats in GameStats)
                {
                    var item = new RecentGameOverview();
                    champions GameChamp = champions.GetChampion((int) Math.Round(stats.Game.ChampionId));
                    item.ChampionImage.Source = GameChamp.icon;
                    item.ChampionNameLabel.Content = GameChamp.displayName;
                    item.ScoreLabel.Content =
                        string.Format("{0}/{1}/{2} ",
                            stats.ChampionsKilled,
                            stats.NumDeaths,
                            stats.Assists);

                    switch (stats.Game.QueueType)
                    {
                        case "NORMAL":
                            item.ScoreLabel.Content += "(Normal)";
                            break;
                        case "NORMAL_3x3":
                            item.ScoreLabel.Content += "(Normal 3v3)";
                            break;
                        case "ARAM_UNRANKED_5x5":
                            item.ScoreLabel.Content += "(ARAM)";
                            break;
                        case "NONE":
                            item.ScoreLabel.Content += "(Custom)";
                            break;
                        case "RANKED_SOLO_5x5":
                            item.ScoreLabel.Content += "(Ranked 5v5)";
                            break;
                        case "RANKED_TEAM_5x5":
                            item.ScoreLabel.Content += "(Ranked Team 5v5)";
                            break;
                        case "RANKED_TEAM_3x3":
                            item.ScoreLabel.Content += "(Ranked Team 3v3)";
                            break;
                        case "CAP_5x5":
                            item.ScoreLabel.Content += "(Team Builder)";
                            break;
                        case "BOT":
                            item.ScoreLabel.Content += "(Bots)";
                            break;
                        default:
                            Client.Log(stats.Game.QueueType);
                            item.ScoreLabel.Content += "Please upload this log to github.";
                            break;
                    }

                    item.CreepScoreLabel.Content = stats.MinionsKilled + " minions";
                    item.DateLabel.Content = stats.Game.CreateDate;
                    item.IpEarnedLabel.Content = "+" + stats.Game.IpEarned + " IP";
                    item.PingLabel.Content = stats.Game.UserServerPing + "ms";

                    var bc = new BrushConverter();
                    var brush = (Brush) bc.ConvertFrom("#FF609E74");

                    if (stats.Lose == 1)
                        brush = (Brush) bc.ConvertFrom("#FF9E6060");

                    else if (stats.Game.IpEarned == 0)
                        brush = (Brush) bc.ConvertFrom("#FFE27100");

                    item.GridView.Background = brush;
                    item.GridView.Width = 250;
                    GamesListView.Items.Add(item);
                }
                if (GamesListView.Items.Count > 0) GamesListView.SelectedIndex = 0;
            }));
        }

        private void GamesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GamesListView.SelectedIndex != -1)
            {
                MatchStats stats = GameStats[GamesListView.SelectedIndex];

                GameStatsListView.Items.Clear();
                PurpleListView.Items.Clear();
                BlueListView.Items.Clear();
                ItemsListView.Items.Clear();

                //Add self to game players
                var img = new Image();
                img.Width = 58;
                img.Height = 58;
                img.Source = champions.GetChampion((int) Math.Round(stats.Game.ChampionId)).icon;
                BlueListView.Items.Add(img);

                foreach (FellowPlayerInfo info in stats.Game.FellowPlayers)
                {
                    img = new Image();
                    img.Width = 58;
                    img.Height = 58;
                    img.Source = champions.GetChampion((int) Math.Round(info.ChampionId)).icon;
                    if (info.TeamId == stats.Game.TeamId)
                    {
                        BlueListView.Items.Add(img);
                    }
                    else
                    {
                        PurpleListView.Items.Add(img);
                    }
                }

                Type classType = typeof (MatchStats);
                foreach (FieldInfo field in classType.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.GetValue(stats) is double)
                    {
                        if ((double) field.GetValue(stats) == 0)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    var item = new ProfilePage.KeyValueItem
                    {
                        Key =
                            Client.TitleCaseString(
                                string.Concat(field.Name.Select(fe => Char.IsUpper(fe) ? " " + fe : fe.ToString()))
                                    .TrimStart(' ')),
                        Value = field.GetValue(stats)
                    };

                    if (((string) item.Key).StartsWith("Item"))
                    {
                        var uriSource =
                            new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "item", item.Value + ".png"),
                                UriKind.Absolute);
                        if (File.Exists(uriSource.AbsolutePath))
                        {
                            img = new Image();
                            img.Width = 58;
                            img.Height = 58;
                            img.Source = new BitmapImage(uriSource);
                            img.Tag = item;
                            img.MouseMove += img_MouseMove;
                            img.MouseLeave += img_MouseLeave;
                            ItemsListView.Items.Add(img);
                        }
                    }
                    else
                    {
                        GameStatsListView.Items.Add(item);
                    }
                }
            }

            if (double.IsNaN(GameKeyHeader.Width))
                GameKeyHeader.Width = GameKeyHeader.ActualWidth;
            if (double.IsNaN(GameValueHeader.Width))
                GameValueHeader.Width = GameValueHeader.ActualWidth;
            GameKeyHeader.Width = double.NaN;
            GameValueHeader.Width = double.NaN;
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
            var item = (Image) sender;
            var playerItem = (ProfilePage.KeyValueItem) item.Tag;
            if (PlayerItem == null)
            {
                PlayerItem = new LargeChatPlayer();
                Client.MainGrid.Children.Add(PlayerItem);

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
                PlayerItem.UsingLegendary.Visibility = Visibility.Hidden;

                string ParsedDescription = Item.description;
                ParsedDescription = ParsedDescription.Replace("<br>", Environment.NewLine);
                ParsedDescription = Regex.Replace(ParsedDescription, "<.*?>", string.Empty);
                PlayerItem.PlayerStatus.Text = ParsedDescription;

                var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "item", Item.id + ".png"),
                    UriKind.RelativeOrAbsolute);
                PlayerItem.ProfileImage.Source = new BitmapImage(uriSource);

                PlayerItem.HorizontalAlignment = HorizontalAlignment.Left;
                PlayerItem.VerticalAlignment = VerticalAlignment.Top;
            }

            Point MouseLocation = e.GetPosition(Client.MainGrid);

            double YMargin = MouseLocation.Y;

            double XMargin = MouseLocation.X;
            if (XMargin + PlayerItem.Width + 10 > Client.MainGrid.ActualWidth)
                XMargin = Client.MainGrid.ActualWidth - PlayerItem.Width - 10;

            PlayerItem.Margin = new Thickness(XMargin + 5, YMargin + 5, 0, 0);
        }
    }

    public class MatchStats
    {
        public double Assists = 0;
        public double BarracksKilled = 0;
        public double ChampionsKilled = 0;
        public double CombatPlayerScore = 0;
        public PlayerGameStats Game = null;
        public double GoldEarned = 0;
        public double Item0 = 0;
        public double Item1 = 0;
        public double Item2 = 0;
        public double Item3 = 0;
        public double Item4 = 0;
        public double Item5 = 0;
        public double Item6 = 0;
        public double LargestCriticalStrike = 0;
        public double LargestKillingSpree = 0;
        public double LargestMultiKill = 0;
        public double Level = 0;
        public double Lose = 0;
        public double MagicDamageDealtPlayer = 0;
        public double MagicDamageDealtToChampions = 0;
        public double MagicDamageTaken = 0;
        public double MinionsKilled = 0;
        public double NeutralMinionsKilled = 0;
        public double NeutralMinionsKilledEnemyJungle = 0;
        public double NeutralMinionsKilledYourJungle = 0;
        public double NodeCapture = 0;
        public double NodeCaptureAssist = 0;
        public double NodeNeutralize = 0;
        public double NodeNeutralizeAssist = 0;
        public double NumDeaths = 0;
        public double ObjectivePlayerScore = 0;
        public double PhysicalDamageDealtPlayer = 0;
        public double PhysicalDamageDealtToChampions = 0;
        public double PhysicalDamageTaken = 0;
        public double SightWardsBoughtInGame = 0;
        public double TeamObjective = 0;
        public double TotalDamageDealt = 0;
        public double TotalDamageDealtToChampions = 0;
        public double TotalDamageTaken = 0;
        public double TotalHeal = 0;
        public double TotalPlayerScore = 0;
        public double TotalScoreRank = 0;
        public double TotalTimeCrowdControlDealt = 0;
        public double TotalTimeSpentDead = 0;
        public double TrueDamageDealtPlayer = 0;
        public double TrueDamageDealtToChampions = 0;
        public double TrueDamageTaken = 0;
        public double TurretsKilled = 0;
        public double VictoryPointTotal = 0;
        public double VisionWardsBoughtInGame = 0;
        public double WardKilled = 0;
        public double WardPlaced = 0;
        public double Win = 0;
    }
}