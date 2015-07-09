using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform;
using LegendaryClient.Logic.MultiUser;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Leagues.xaml
    /// </summary>
    public partial class Leagues
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof (Leagues));
        private SummonerLeaguesDTO myLeagues;
        private string queue;
        private AggregatedStats selectedAggregatedStats;
        private string selectedRank;
        static UserClient UserClient = UserList.users[Client.Current];

        public Leagues()
        {
            InitializeComponent();
        }

        public void Update(SummonerLeaguesDTO result)
        {
            myLeagues = result;
            foreach (
                var leagues in myLeagues.SummonerLeagues.Where(leagues => leagues.Queue == "RANKED_SOLO_5x5"))
            {
                selectedRank = leagues.RequestorsRank;
            }

            queue = "RANKED_SOLO_5x5";
            RenderLeague();
        }

        public void RenderLeague()
        {
            if (myLeagues == null)
            {
                //MessageBox.Show("You are not in League.");
                var overlay = new MessageOverlay
                {
                    MessageTextBox =
                    {
                        Text = "You are not in League."
                    },
                    MessageTitle = { Content = "Not in League" }
                };
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
                return;
            }
            LeaguesListView.Items.Clear();
            foreach (var leagues in myLeagues.SummonerLeagues.Where(leagues => leagues.Queue == queue))
            {
                switch (selectedRank)
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

                CurrentLeagueLabel.Content = leagues.Tier + " " + selectedRank;
                CurrentLeagueNameLabel.Content = leagues.Name;
                var players =
                    leagues.Entries.OrderBy(o => o.LeaguePoints).Where(item => item.Rank == selectedRank).ToList();
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

                    var miniSeries = player.MiniSeries;
                    if (miniSeries != null)
                    {
                        item.LpLabel.Content = player.MiniSeries.Progress.Replace('N', '-');
                    }

                    LeaguesListView.Items.Add(item);
                }
            }
            LeaguesListView.SelectedIndex = 0;
        }

        private void DownTierButton_Click(object sender, RoutedEventArgs e)
        {
            switch (selectedRank)
            {
                case "I":
                    selectedRank = "II";
                    break;
                case "II":
                    selectedRank = "III";
                    break;
                case "III":
                    selectedRank = "IV";
                    break;
                case "IV":
                    selectedRank = "V";
                    break;
            }

            RenderLeague();
        }

        private void UpTierButton_Click(object sender, RoutedEventArgs e)
        {
            switch (selectedRank)
            {
                case "V":
                    selectedRank = "IV";
                    break;
                case "IV":
                    selectedRank = "III";
                    break;
                case "III":
                    selectedRank = "II";
                    break;
                case "II":
                    selectedRank = "I";
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

            var x = await UserClient.calls.GetSummonerByName((string) item.PlayerLabel.Content);
            GotStats(await UserClient.calls.GetAggregatedStats(x.AcctId, "CLASSIC", UserClient.LoginPacket.ClientSystemStates.currentSeason.ToString()));
        }

        private void GotStats(AggregatedStats stats)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                selectedAggregatedStats = stats;

                ViewAggregatedStatsButton.IsEnabled = false;
                TopChampionsListView.Items.Clear();
                var championStats = new List<AggregatedChampion>();
                var i = 0;
                if (selectedAggregatedStats.LifetimeStatistics == null)
                {
                    return;
                }

                if (!selectedAggregatedStats.LifetimeStatistics.Any())
                {
                    return;
                }

                foreach (var stat in selectedAggregatedStats.LifetimeStatistics)
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
                    if (f == null)
                        Client.Log(fieldName, "NewStatType");
                    else
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
