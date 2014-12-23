using System.Globalization;

#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Statistics;

#endregion

//While this is in the Profile folder, the namespace is LegendaryClient.Windows

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for AggregatedStatsOverlay.xaml
    /// </summary>
    public partial class AggregatedStatsOverlay
    {
        //private static readonly ILog log = LogManager.GetLogger(typeof (AggregatedStatsOverlay));
        private readonly AggregatedChampion AllStats;
        private readonly List<AggregatedChampion> ChampionStats;
        private readonly Boolean IsOwnPlayer;
        private AggregatedChampion SelectedStats;

        public AggregatedStatsOverlay(AggregatedStats stats, Boolean isSelf)
        {
            InitializeComponent();
            Change();

            IsOwnPlayer = isSelf;
            AllStats = new AggregatedChampion();
            ChampionStats = new List<AggregatedChampion>();
            ParseStats(stats);
            SelectedStats = AllStats;
            HideGrid.Visibility = Visibility.Visible;
            DisplayStats();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void DisplayStats()
        {
            StatsListView.Items.Clear();
            GamesLabel.Content = SelectedStats.TotalSessionsPlayed;
            WinsLabel.Content = SelectedStats.TotalSessionsWon;
            LossesLabel.Content = SelectedStats.TotalSessionsPlayed - SelectedStats.TotalSessionsWon;
            if (SelectedStats.TotalSessionsPlayed != 0)
                RatioLabel.Content = string.Format("{0:0.00}%",
                    (SelectedStats.TotalSessionsWon/SelectedStats.TotalSessionsPlayed)*100);
            else
                RatioLabel.Content = "100%";

            var classType = typeof (AggregatedChampion);
            foreach (
                var item in
                    from field in classType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                    where (double) field.GetValue(SelectedStats) != 0
                    select new ProfilePage.KeyValueItem
                    {
                        Key =
                            Client.TitleCaseString(
                                string.Concat(
                                    field.Name.Select(
                                        e => Char.IsUpper(e) ? " " + e : e.ToString(CultureInfo.InvariantCulture)))
                                    .TrimStart(' ')),
                        Value = field.GetValue(SelectedStats)
                    })
            {
                if (((string) item.Key).Contains("Time"))
                {
                    var span = TimeSpan.FromMinutes((double) item.Value);
                    if (((string) item.Key).Contains("Time Spent Living"))
                        span = TimeSpan.FromSeconds((double) item.Value);

                    item.Value = string.Format("{0:D2}d:{1:D2}m:{2:D2}s", span.Days, span.Minutes, span.Seconds);
                }

                StatsListView.Items.Add(item);
            }

            if (double.IsNaN(KeyHeader.Width))
                KeyHeader.Width = KeyHeader.ActualWidth;

            if (double.IsNaN(ValueHeader.Width))
                ValueHeader.Width = ValueHeader.ActualWidth;

            KeyHeader.Width = double.NaN;
            ValueHeader.Width = double.NaN;
        }

        private void ParseStats(AggregatedStats stats)
        {
            foreach (var stat in stats.LifetimeStatistics)
            {
                var champion = ChampionStats.Find(x => x.ChampionId == stat.ChampionId);
                if (champion == null)
                {
                    champion = new AggregatedChampion
                    {
                        ChampionId = stat.ChampionId
                    };
                    ChampionStats.Add(champion);
                }

                var type = typeof (AggregatedChampion);
                var fieldName = Client.TitleCaseString(stat.StatType.Replace('_', ' ')).Replace(" ", "");
                var f = type.GetField(fieldName);
                f.SetValue(champion, stat.Value);
            }

            ChampionStats.Sort((x, y) => y.TotalSessionsPlayed.CompareTo(x.TotalSessionsPlayed));

            //AllStats = ChampionStats;

            foreach (var championStat in ChampionStats)
            {
                if (championStat.ChampionId == 0)
                    continue;

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
                return;

            SelectedStats = (AggregatedChampion) ((ListViewItem) ChampionsListView.SelectedItem).Tag;
            if (!IsOwnPlayer)
                HideGrid.Visibility = Visibility.Hidden;

            DisplayStats();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedStats = AllStats;
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
        public double TotalDoubleKills = 0;
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
    }
}