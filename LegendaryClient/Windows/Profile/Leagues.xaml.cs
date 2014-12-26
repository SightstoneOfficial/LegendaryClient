#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Platform.Leagues.Client.Dto;
using PVPNetConnect.RiotObjects.Platform.Statistics;

#endregion

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Leagues.xaml
    /// </summary>
    public partial class Leagues
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof (Leagues));
        private SummonerLeaguesDTO _myLeagues;
        private string _queue;
        private AggregatedStats _selectedAggregatedStats;
        private string _selectedRank;

        public Leagues()
        {
            InitializeComponent();
            Change();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        public void Update(SummonerLeaguesDTO result)
        {
            _myLeagues = result;
            foreach (
                var leagues in _myLeagues.SummonerLeagues.Where(leagues => leagues.Queue == "RANKED_SOLO_5x5"))
            {
                _selectedRank = leagues.RequestorsRank;
            }

            _queue = "RANKED_SOLO_5x5";
            RenderLeague();
        }

        public void RenderLeague()
        {
            LeaguesListView.Items.Clear();
            foreach (var leagues in _myLeagues.SummonerLeagues.Where(leagues => leagues.Queue == _queue))
            {
                switch (_selectedRank)
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

                CurrentLeagueLabel.Content = leagues.Tier + " " + _selectedRank;
                CurrentLeagueNameLabel.Content = leagues.Name;
                var players =
                    leagues.Entries.OrderBy(o => o.LeaguePoints).Where(item => item.Rank == _selectedRank).ToList();
                players.Reverse();
                var i = 0;
                foreach (var player in players)
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
                    {
                        item.RecruitLabel.Visibility = Visibility.Visible;
                    }

                    if (player.Veteran)
                    {
                        item.VeteranLabel.Visibility = Visibility.Visible;
                    }

                    if (player.HotStreak)
                    {
                        item.HotStreakLabel.Visibility = Visibility.Visible;
                    }

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
            LeaguesListView.SelectedIndex = 0;
        }

        private void DownTierButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_selectedRank)
            {
                case "I":
                    _selectedRank = "II";
                    break;
                case "II":
                    _selectedRank = "III";
                    break;
                case "III":
                    _selectedRank = "IV";
                    break;
                case "IV":
                    _selectedRank = "V";
                    break;
            }

            RenderLeague();
        }

        private void UpTierButton_Click(object sender, RoutedEventArgs e)
        {
            switch (_selectedRank)
            {
                case "V":
                    _selectedRank = "IV";
                    break;
                case "IV":
                    _selectedRank = "III";
                    break;
                case "III":
                    _selectedRank = "II";
                    break;
                case "II":
                    _selectedRank = "I";
                    break;
            }

            RenderLeague();
        }

        private async void LeaguesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LeaguesListView.SelectedItem == null)
            {
                return;
            }

            var item = (LeagueItem) LeaguesListView.SelectedItem;
            PlayerLabel.Content = item.PlayerLabel.Content;

            var x = await Client.PVPNet.GetSummonerByName((string) item.PlayerLabel.Content);
            Client.PVPNet.GetAggregatedStats(x.AcctId, "CLASSIC", "3", GotStats);
        }

        private void GotStats(AggregatedStats stats)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                _selectedAggregatedStats = stats;

                ViewAggregatedStatsButton.IsEnabled = false;
                TopChampionsListView.Items.Clear();
                var championStats = new List<AggregatedChampion>();
                var i = 0;
                if (_selectedAggregatedStats.LifetimeStatistics == null)
                {
                    return;
                }

                if (!_selectedAggregatedStats.LifetimeStatistics.Any())
                {
                    return;
                }

                foreach (var stat in _selectedAggregatedStats.LifetimeStatistics)
                {
                    var champion = championStats.Find(x => Math.Abs(x.ChampionId - stat.ChampionId) < .00001);
                    if (champion == null)
                    {
                        champion = new AggregatedChampion
                        {
                            ChampionId = stat.ChampionId
                        };
                        championStats.Add(champion);
                    }

                    var type = typeof (AggregatedChampion);
                    var fieldName = Client.TitleCaseString(stat.StatType.Replace('_', ' ')).Replace(" ", string.Empty);
                    var f = type.GetField(fieldName);

                    f.SetValue(champion, stat.Value);
                }

                championStats.Sort((x, y) => y.TotalSessionsPlayed.CompareTo(x.TotalSessionsPlayed));

                foreach (var info in championStats.TakeWhile(info => i++ <= 6))
                {
                    ViewAggregatedStatsButton.IsEnabled = true;
                    if (!(Math.Abs(info.ChampionId) > 0))
                    {
                        continue;
                    }

                    var champion = champions.GetChampion(Convert.ToInt32(info.ChampionId));
                    var player = new ChatPlayer
                    {
                        LevelLabel = {Visibility = Visibility.Hidden},
                        PlayerName = {Content = champion.displayName},
                        PlayerStatus = {Content = info.TotalSessionsPlayed + " games played"},
                        ProfileImage = {Source = champions.GetChampion(champion.id).icon},
                        Background = new SolidColorBrush(Color.FromArgb(102, 80, 80, 80)),
                        Width = 270
                    };

                    TopChampionsListView.Items.Add(player);
                }
            }));
        }
    }
}