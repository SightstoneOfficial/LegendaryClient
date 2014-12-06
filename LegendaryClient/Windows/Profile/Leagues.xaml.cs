using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
            foreach (LeagueListDTO leagues in MyLeagues.SummonerLeagues)
            {
                if (leagues.Queue == "RANKED_SOLO_5x5")
                {
                    SelectedRank = leagues.RequestorsRank;
                }
            }
            Queue = "RANKED_SOLO_5x5";
            RenderLeague();
        }

        public void RenderLeague()
        {
            LeaguesListView.Items.Clear();
            foreach (LeagueListDTO leagues in MyLeagues.SummonerLeagues)
            {
                if (leagues.Queue == Queue)
                {
                    if (SelectedRank == "V")
                    {
                        UpTierButton.IsEnabled = true;
                        DownTierButton.IsEnabled = false;
                    }
                    else if (SelectedRank == "I")
                    {
                        UpTierButton.IsEnabled = false;
                        DownTierButton.IsEnabled = true;
                    }
                    else
                    {
                        UpTierButton.IsEnabled = true;
                        DownTierButton.IsEnabled = true;
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
                        var item = new LeagueItem();
                        item.PlayerRankLabel.Content = i;
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
                        {
                            item.LpLabel.Content = ((string) miniSeries["progress"]).Replace('N', '-');
                        }

                        LeaguesListView.Items.Add(item);
                    }
                }
            }
            LeaguesListView.SelectedIndex = 0;
        }

        private void DownTierButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRank == "I")
                SelectedRank = "II";
            else if (SelectedRank == "II")
                SelectedRank = "III";
            else if (SelectedRank == "III")
                SelectedRank = "IV";
            else if (SelectedRank == "IV")
                SelectedRank = "V";
            RenderLeague();
        }

        private void UpTierButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRank == "V")
                SelectedRank = "IV";
            else if (SelectedRank == "IV")
                SelectedRank = "III";
            else if (SelectedRank == "III")
                SelectedRank = "II";
            else if (SelectedRank == "II")
                SelectedRank = "I";
            RenderLeague();
        }

        private async void LeaguesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LeaguesListView.SelectedItem != null)
            {
                var item = (LeagueItem) LeaguesListView.SelectedItem;
                PlayerLabel.Content = item.PlayerLabel.Content;
                PublicSummoner x = await Client.PvpNet.GetSummonerByName((string) item.PlayerLabel.Content);
                Client.PvpNet.GetAggregatedStats(x.AcctId, "CLASSIC", "3", GotStats);
            }
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

                if (SelectedAggregatedStats.LifetimeStatistics != null)
                {
                    if (SelectedAggregatedStats.LifetimeStatistics.Count() > 0)
                    {
                        foreach (AggregatedStat stat in SelectedAggregatedStats.LifetimeStatistics)
                        {
                            AggregatedChampion Champion = null;
                            Champion = ChampionStats.Find(x => x.ChampionId == stat.ChampionId);
                            if (Champion == null)
                            {
                                Champion = new AggregatedChampion();
                                Champion.ChampionId = stat.ChampionId;
                                ChampionStats.Add(Champion);
                            }

                            Type type = typeof (AggregatedChampion);
                            string fieldName = Client.TitleCaseString(stat.StatType.Replace('_', ' ')).Replace(" ", "");
                            FieldInfo f = type.GetField(fieldName);
                            f.SetValue(Champion, stat.Value);
                        }

                        ChampionStats.Sort((x, y) => y.TotalSessionsPlayed.CompareTo(x.TotalSessionsPlayed));

                        foreach (AggregatedChampion info in ChampionStats)
                        {
                            if (i++ > 6)
                                break;
                            ViewAggregatedStatsButton.IsEnabled = true;
                            if (info.ChampionId != 0.0)
                            {
                                var player = new ChatPlayer();
                                Logic.SQLite.Champions Champion = Logic.SQLite.Champions.GetChampion(Convert.ToInt32(info.ChampionId));
                                player.LevelLabel.Visibility = Visibility.Hidden;
                                player.PlayerName.Content = Champion.DisplayName;
                                player.PlayerStatus.Content = info.TotalSessionsPlayed + " games played";
                                player.ProfileImage.Source = Logic.SQLite.Champions.GetChampion(Champion.Id).Icon;
                                TopChampionsListView.Items.Add(player);
                            }
                        }
                    }
                }
            }));
        }
    }
}