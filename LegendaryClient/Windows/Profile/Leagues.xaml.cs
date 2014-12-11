using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using log4net;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Leagues.Pojo;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Leagues.xaml
    /// </summary>
    public partial class Leagues
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Leagues));
        private SummonerLeaguesDTO MyLeagues;
        private string Queue;
        private AggregatedStats SelectedAggregatedStats;
        private string SelectedRank;

        public Leagues()
        {
            InitializeComponent();
        }

        public void Update(SummonerLeaguesDTO result)
        {
            MyLeagues = result;
            foreach (
                LeagueListDTO leagues in MyLeagues.SummonerLeagues.Where(leagues => leagues.Queue == "RANKED_SOLO_5x5"))
                SelectedRank = leagues.RequestorsRank;

            Queue = "RANKED_SOLO_5x5";
            RenderLeague();
        }

        public void RenderLeague()
        {
            LeaguesListView.Items.Clear();
            foreach (LeagueListDTO leagues in MyLeagues.SummonerLeagues.Where(leagues => leagues.Queue == Queue))
            {
                switch (SelectedRank)
                {
                    case "V":
                        UpTierButton.IsEnabled = true;
                        DownTierButton.IsEnabled = false;
                        break;
                    case "I":
                        UpTierButton.IsEnabled = false;
                        DownTierButton.IsEnabled = true;
                        break;
                    default:
                        UpTierButton.IsEnabled = true;
                        DownTierButton.IsEnabled = true;
                        break;
                }

                CurrentLeagueLabel.Content = leagues.Tier + " " + SelectedRank;
                CurrentLeagueNameLabel.Content = leagues.Name;
                List<LeagueItemDTO> players =
                    leagues.Entries.OrderBy(o => o.LeaguePoints).Where(item => item.Rank == SelectedRank).ToList();
                players.Reverse();
                int i = 0;
                foreach (LeagueItemDTO player in players)
                {
                    i++;
                    var item = new LeagueItem
                    {
                        PlayerRankLabel = {Content = i}
                    };
                    if (i - player.PreviousDayLeaguePosition != 0)
                    {
                        item.RankChangeLabel.Content = i - player.PreviousDayLeaguePosition;
                    }

                    item.PlayerLabel.Content = player.PlayerOrTeamName;
                    if (player.FreshBlood)
                        item.RecruitLabel.Visibility = Visibility.Visible;
                    if (player.Veteran)
                        item.VeteranLabel.Visibility = Visibility.Visible;
                    if (player.HotStreak)
                        item.HotStreakLabel.Visibility = Visibility.Visible;

                    item.WinsLabel.Content = player.Wins;
                    item.LpLabel.Content = player.LeaguePoints;

                    var miniSeries = player.MiniSeries as TypedObject;
                    if (miniSeries != null)
                        item.LpLabel.Content = ((string) miniSeries["progress"]).Replace('N', '-');

                    LeaguesListView.Items.Add(item);
                }
            }
            LeaguesListView.SelectedIndex = 0;
        }

        private void DownTierButton_Click(object sender, RoutedEventArgs e)
        {
            switch (SelectedRank)
            {
                case "I":
                    SelectedRank = "II";
                    break;
                case "II":
                    SelectedRank = "III";
                    break;
                case "III":
                    SelectedRank = "IV";
                    break;
                case "IV":
                    SelectedRank = "V";
                    break;
            }

            RenderLeague();
        }

        private void UpTierButton_Click(object sender, RoutedEventArgs e)
        {
            switch (SelectedRank)
            {
                case "V":
                    SelectedRank = "IV";
                    break;
                case "IV":
                    SelectedRank = "III";
                    break;
                case "III":
                    SelectedRank = "II";
                    break;
                case "II":
                    SelectedRank = "I";
                    break;
            }

            RenderLeague();
        }

        private async void LeaguesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LeaguesListView.SelectedItem == null)
                return;

            var item = (LeagueItem) LeaguesListView.SelectedItem;
            PlayerLabel.Content = item.PlayerLabel.Content;
            PublicSummoner x = await Client.PVPNet.GetSummonerByName((string) item.PlayerLabel.Content);
            Client.PVPNet.GetAggregatedStats(x.AcctId, "CLASSIC", "3", GotStats);
        }

        private void GotStats(AggregatedStats stats)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                SelectedAggregatedStats = stats;

                ViewAggregatedStatsButton.IsEnabled = false;
                TopChampionsListView.Items.Clear();
                var ChampionStats = new List<AggregatedChampion>();
                int i = 0;

                if (SelectedAggregatedStats.LifetimeStatistics == null)
                    return;

                if (!SelectedAggregatedStats.LifetimeStatistics.Any())
                    return;

                foreach (AggregatedStat stat in SelectedAggregatedStats.LifetimeStatistics)
                {
                    AggregatedChampion Champion = ChampionStats.Find(x => x.ChampionId == stat.ChampionId);
                    if (Champion == null)
                    {
                        Champion = new AggregatedChampion
                        {
                            ChampionId = stat.ChampionId
                        };
                        ChampionStats.Add(Champion);
                    }

                    Type type = typeof (AggregatedChampion);
                    string fieldName = Client.TitleCaseString(stat.StatType.Replace('_', ' ')).Replace(" ", "");
                    FieldInfo f = type.GetField(fieldName);

                    f.SetValue(Champion, stat.Value);
                }

                ChampionStats.Sort((x, y) => y.TotalSessionsPlayed.CompareTo(x.TotalSessionsPlayed));

                foreach (AggregatedChampion info in ChampionStats.TakeWhile(info => i++ <= 6))
                {
                    ViewAggregatedStatsButton.IsEnabled = true;
                    if (!(Math.Abs(info.ChampionId) > 0))
                        continue;

                    champions Champion = champions.GetChampion(Convert.ToInt32(info.ChampionId));
                    var player = new ChatPlayer
                    {
                        LevelLabel = {Visibility = Visibility.Hidden},
                        PlayerName = {Content = Champion.displayName},
                        PlayerStatus = {Content = info.TotalSessionsPlayed + " games played"},
                        ProfileImage = {Source = champions.GetChampion(Champion.id).icon},
                        Background = new SolidColorBrush(Color.FromArgb(102, 80, 80, 80)),
                        Width = 270
                    };

                    TopChampionsListView.Items.Add(player);
                }
            }));
        }
    }
}