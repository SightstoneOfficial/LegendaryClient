using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

//While this is in the Profile folder, the namespace is LegendaryClient.Windows
namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for AggregatedStatsOverlay.xaml
    /// </summary>
    public partial class AggregatedStatsOverlay : Page
    {
        Boolean IsOwnPlayer;
        List<AggregatedChampion> ChampionStats;
        AggregatedChampion AllStats;

        AggregatedChampion SelectedStats;

        public AggregatedStatsOverlay(AggregatedStats stats, Boolean IsSelf)
        {
            InitializeComponent();
            IsOwnPlayer = IsSelf;
            AllStats = new AggregatedChampion();
            ChampionStats = new List<AggregatedChampion>();
            ParseStats(stats);
            SelectedStats = AllStats;
            HideGrid.Visibility = System.Windows.Visibility.Visible;
            DisplayStats();
        }

        private void DisplayStats()
        {
            StatsListView.Items.Clear();
            GamesLabel.Content = SelectedStats.TotalSessionsPlayed;
            WinsLabel.Content = SelectedStats.TotalSessionsWon;
            LossesLabel.Content = SelectedStats.TotalSessionsPlayed - SelectedStats.TotalSessionsWon;
            if (SelectedStats.TotalSessionsPlayed != 0)
                RatioLabel.Content = string.Format("{0:0.00}%", (SelectedStats.TotalSessionsWon / SelectedStats.TotalSessionsPlayed) * 100);
            else
                RatioLabel.Content = "100%";

            Type classType = typeof(AggregatedChampion);
            foreach (FieldInfo field in classType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if ((double)field.GetValue(SelectedStats) == 0)
                    continue;
                ProfilePage.KeyValueItem item = new ProfilePage.KeyValueItem
                {
                    Key = Client.TitleCaseString(string.Concat(field.Name.Select(e => Char.IsUpper(e) ? " " + e : e.ToString())).TrimStart(' ')),
                    Value = field.GetValue(SelectedStats)
                };

                if (((string)item.Key).Contains("Time"))
                {
                    TimeSpan span = TimeSpan.FromMinutes((double)item.Value);
                    if (((string)item.Key).Contains("Time Spent Living"))
                        span = TimeSpan.FromSeconds((double)item.Value);
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
            foreach (AggregatedStat stat in stats.LifetimeStatistics)
            {
                AggregatedChampion Champion = null;
                Champion = ChampionStats.Find(x => x.ChampionId == stat.ChampionId);
                if (Champion == null)
                {
                    Champion = new AggregatedChampion();
                    Champion.ChampionId = stat.ChampionId;
                    ChampionStats.Add(Champion);
                }

                var type = typeof(AggregatedChampion);
                string fieldName = Client.TitleCaseString(stat.StatType.Replace('_', ' ')).Replace(" ", "");
                var f = type.GetField(fieldName);
                f.SetValue(Champion, stat.Value);
            }

            ChampionStats.Sort((x, y) => y.TotalSessionsPlayed.CompareTo(x.TotalSessionsPlayed));

            AllStats = ChampionStats[0];

            foreach (AggregatedChampion ChampionStat in ChampionStats)
            {
                if (ChampionStat.ChampionId != 0)
                {
                    ProfileChampionImage championImage = new ProfileChampionImage();
                    champions champion = champions.GetChampion((int)ChampionStat.ChampionId);
                    championImage.DataContext = champion;

                    championImage.Width = 96;
                    championImage.Height = 84;
                    championImage.Tag = ChampionStat;
                    ChampionsListView.Items.Add(championImage);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }

        private void ChampionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ProfileChampionImage)ChampionsListView.SelectedItem).Tag != null)
            {
                SelectedStats = (AggregatedChampion)((ProfileChampionImage)ChampionsListView.SelectedItem).Tag;
                if (!IsOwnPlayer)
                    HideGrid.Visibility = System.Windows.Visibility.Hidden;
                DisplayStats();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedStats = AllStats;
            HideGrid.Visibility = System.Windows.Visibility.Visible;
            DisplayStats();
        }
    }

    public class AggregatedChampion
    {
        public double ChampionId = 0;
        public double TotalSessionsPlayed = 0;
        public double TotalSessionsLost = 0;
        public double TotalSessionsWon = 0;
        public double TotalChampionKills = 0;
        public double TotalDamageDealt = 0;
        public double TotalDamageTaken = 0;
        public double MostChampionKillsPerSession = 0;
        public double TotalMinionKills = 0;
        public double TotalDoubleKills = 0;
        public double TotalTripleKills = 0;
        public double TotalQuadraKills = 0;
        public double TotalPentaKills = 0;
        public double TotalDeathsPerSession = 0;
        public double TotalGoldEarned = 0;
        public double MostSpellsCast = 0;
        public double TotalTurretsKilled = 0;
        public double TotalMagicDamageDealt = 0;
        public double TotalPhysicalDamageDealt = 0;
        public double TotalAssists = 0;
        public double TotalTimeSpentDead = 0;
        public double TotalFirstBlood = 0;
        public double TotalUnrealKills = 0;
        public double MaxNumDeaths = 0;
        public double MaxChampionsKilled = 0;
        public double BotGamesPlayed = 0;
        public double RankedSoloGamesPlayed = 0;
        public double TotalHeal = 0;
        public double MaxTimeSpentLiving = 0;
        public double MaxLargestCriticalStrike = 0;
        public double TotalNeutralMinionsKilled = 0;
        public double TotalLeaves = 0;
        public double RankedPremadeGamesPlayed = 0;
        public double MaxTimePlayed = 0;
        public double KillingSpree = 0;
        public double MaxLargestKillingSpree = 0;
        public double NormalGamesPlayed = 0;
    }
}