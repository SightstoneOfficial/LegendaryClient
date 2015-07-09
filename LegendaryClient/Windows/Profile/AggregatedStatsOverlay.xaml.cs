using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using LegendaryClient.Logic.Riot.Platform;
using LegendaryClient.Logic.MultiUser;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for AggregatedStatsOverlay.xaml
    /// </summary>
    public partial class AggregatedStatsOverlay
    {
        private AggregatedChampion selectedStats;
        //private static readonly ILog log = LogManager.GetLogger(typeof (AggregatedStatsOverlay));
        private readonly AggregatedChampion allStats;
        private readonly List<AggregatedChampion> championStats;
        private readonly bool isOwnPlayer;

        public AggregatedStatsOverlay(AggregatedStats stats, bool isSelf)
        {
            InitializeComponent();
            isOwnPlayer = isSelf;
            allStats = new AggregatedChampion();
            championStats = new List<AggregatedChampion>();
            ParseStats(stats);
            selectedStats = allStats;
            HideGrid.Visibility = Visibility.Visible;
            DisplayStats();
        }

        private void DisplayStats()
        {
            StatsListView.Items.Clear();
            GamesLabel.Content = selectedStats.TotalSessionsPlayed;
            WinsLabel.Content = selectedStats.TotalSessionsWon;
            LossesLabel.Content = selectedStats.TotalSessionsPlayed - selectedStats.TotalSessionsWon;
            if (Math.Abs(selectedStats.TotalSessionsPlayed) >= 1)
            {
                RatioLabel.Content = string.Format("{0:0.00}%",
                    (selectedStats.TotalSessionsWon/selectedStats.TotalSessionsPlayed)*100);
            }
            else
            {
                RatioLabel.Content = "100%";
            }

            var classType = typeof (AggregatedChampion);
            foreach (
                var item in
                    from field in classType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    where Math.Abs((double) field.GetValue(selectedStats)) >= 1
                    select new ProfilePage.KeyValueItem
                    {
                        Key =
                            Client.TitleCaseString(
                                string.Concat(
                                    field.Name.Select(
                                        e => Char.IsUpper(e) ? " " + e : e.ToString(CultureInfo.InvariantCulture)))
                                    .TrimStart(' ')),
                        Value = field.GetValue(selectedStats)
                    })
            {
                if (((string) item.Key).Contains("Time"))
                {
                    var span = TimeSpan.FromMinutes((double) item.Value);
                    if (((string) item.Key).Contains("Time Spent Living"))
                    {
                        span = TimeSpan.FromSeconds((double) item.Value);
                    }

                    item.Value = string.Format("{0:D2}d:{1:D2}m:{2:D2}s", span.Days, span.Minutes, span.Seconds);
                }

                StatsListView.Items.Add(item);
            }

            if (double.IsNaN(KeyHeader.Width))
            {
                KeyHeader.Width = KeyHeader.ActualWidth;
            }

            if (double.IsNaN(ValueHeader.Width))
            {
                ValueHeader.Width = ValueHeader.ActualWidth;
            }

            KeyHeader.Width = double.NaN;
            ValueHeader.Width = double.NaN;
        }

        private void ParseStats(AggregatedStats stats)
        {
            foreach (var stat in stats.LifetimeStatistics)
            {
                var champion =
                    championStats.Find(x => Math.Abs(x.ChampionId - stat.ChampionId) < Math.Abs(x.ChampionId*.00001));
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

            //AllStats = ChampionStats;

            foreach (var championStat in championStats)
            {
                if (Math.Abs(championStat.ChampionId) < .00001)
                {
                    continue;
                }

                var item = new ListViewItem();
                var championImage = new ProfileChampionImage();
                var champ = champions.GetChampion((int) championStat.ChampionId);
                championImage.ChampImage.Source = champ.icon;
                championImage.ChampName.Content = champ.displayName;
                championImage.Width = 96;
                championImage.Height = 84;
                item.Tag = championStat;
                item.Content = championImage.Content;
                ChampionsListView.Items.Add(item);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }

        private void ChampionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListViewItem) ChampionsListView.SelectedItem).Tag == null)
            {
                return;
            }

            selectedStats = (AggregatedChampion) ((ListViewItem) ChampionsListView.SelectedItem).Tag;
            if (!isOwnPlayer)
            {
                HideGrid.Visibility = Visibility.Hidden;
            }

            DisplayStats();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            selectedStats = allStats;
            HideGrid.Visibility = Visibility.Visible;
            DisplayStats();
        }
    }

    public class AggregatedChampion
    {
        public double BotGamesPlayed = 0;
        public double ChampionId;
        public double KillingSpree = 0;
        public double MaxChampionsKilled = 0;
        public double MaxLargestCriticalStrike = 0;
        public double MaxLargestKillingSpree = 0;
        public double MaxNumDeaths = 0;
        public double MaxTimePlayed = 0;
        public double MaxTimeSpentLiving = 0;
        public double MostChampionKillsPerSession = 0;
        public double MostSpellsCast = 0;
        public double NormalGamesPlayed = 0;
        public double RankedPremadeGamesPlayed = 0;
        public double RankedSoloGamesPlayed = 0;
        public double TotalAssists = 0;
        public double TotalChampionKills = 0;
        public double TotalDamageDealt = 0;
        public double TotalDamageTaken = 0;
        public double TotalDeathsPerSession = 0;
        public double TotaldoubleKills = 0;
        public double TotalFirstBlood = 0;
        public double TotalGoldEarned = 0;
        public double TotalHeal = 0;
        public double TotalLeaves = 0;
        public double TotalMagicDamageDealt = 0;
        public double TotalMinionKills = 0;
        public double TotalNeutralMinionsKilled = 0;
        public double TotalPentaKills = 0;
        public double TotalPhysicalDamageDealt = 0;
        public double TotalQuadraKills = 0;
        public double TotalSessionsLost = 0;
        public double TotalSessionsPlayed = 0;
        public double TotalSessionsWon = 0;
        public double TotalTimeSpentDead = 0;
        public double TotalTripleKills = 0;
        public double TotalTurretsKilled = 0;
        public double TotalUnrealKills = 0;
        public double TotalDoubleKills = 0;
    }
}