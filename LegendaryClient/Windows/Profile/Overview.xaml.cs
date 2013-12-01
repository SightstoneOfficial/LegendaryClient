using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using PVPNetConnect.RiotObjects.Platform.Harassment;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using LegendaryClient.Controls;
using LegendaryClient.Logic.SQLite;
using System.Windows.Media.Imaging;
using System.IO;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    /// Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview : Page
    {
        List<PlayerStatSummary> Summaries = new List<PlayerStatSummary>();

        public Overview()
        {
            InitializeComponent();
        }

        public async void Update(double SummonerId, double AccountId)
        {
            LcdsResponseString TotalKudos = await Client.PVPNet.CallKudos("{\"commandName\":\"TOTALS\",\"summonerId\": " + SummonerId + "}");
            RenderKudos(TotalKudos);
            ChampionStatInfo[] TopChampions = await Client.PVPNet.RetrieveTopPlayedChampions(AccountId, "CLASSIC");
            RenderTopPlayedChampions(TopChampions);
            Client.PVPNet.RetrievePlayerStatsByAccountId(AccountId, "3", new PlayerLifetimeStats.Callback(GotPlayerStats));
        }

        public void RenderKudos(LcdsResponseString TotalKudos)
        {
            KudosListView.Items.Clear();
            TotalKudos.Value = TotalKudos.Value.Replace("{\"totals\":[0,", "").Replace("]}", "");
            string[] Kudos = TotalKudos.Value.Split(',');
            KudosItem item = new KudosItem("Friendly", Kudos[0]);
            KudosListView.Items.Add(item);
            item = new KudosItem("Helpful", Kudos[1]);
            KudosListView.Items.Add(item);
            item = new KudosItem("Teamwork", Kudos[2]);
            KudosListView.Items.Add(item);
            item = new KudosItem("Honorable Opponent", Kudos[3]);
            KudosListView.Items.Add(item);
        }

        public void RenderTopPlayedChampions(ChampionStatInfo[] TopChampions)
        {
            TopChampionsListView.Items.Clear();
            if (TopChampions.Count() > 0)
            {
                TopChampionsLabel.Content = "Top Champions (" + TopChampions[0].TotalGamesPlayed + " Ranked Games)";
                foreach (ChampionStatInfo info in TopChampions)
                {
                    if (info.ChampionId != 0.0)
                    {
                        ChatPlayer player = new ChatPlayer();
                        champions Champion = champions.GetChampion(Convert.ToInt32(info.ChampionId));
                        player.LevelLabel.Visibility = System.Windows.Visibility.Hidden;
                        player.PlayerName.Content = Champion.displayName;
                        player.PlayerStatus.Content = info.TotalGamesPlayed + " games played";
                        player.ProfileImage.Source = champions.GetChampion(Champion.id).icon;
                        TopChampionsListView.Items.Add(player);
                    }
                }
            }
        }

        public void GotPlayerStats(PlayerLifetimeStats stats)
        {
            Summaries = new List<PlayerStatSummary>();
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                StatsComboBox.Items.Clear();
                StatsListView.Items.Clear();
                foreach (PlayerStatSummary x in stats.PlayerStatSummaries.PlayerStatSummarySet)
                {
                    if (x.AggregatedStats.Stats.Count > 0)
                    {
                        Summaries.Add(x);
                        string SummaryString = x.PlayerStatSummaryTypeString;
                        SummaryString = string.Concat(SummaryString.Select(e => Char.IsUpper(e) ? " " + e : e.ToString())).TrimStart(' ');
                        SummaryString = SummaryString.Replace("Odin", "Dominion");
                        SummaryString = SummaryString.Replace("x", "v");
                        StatsComboBox.Items.Add(SummaryString);
                    }
                }
            }));
        }

        private void StatsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatsComboBox.SelectedIndex != -1)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    StatsListView.Items.Clear();
                    PlayerStatSummary GameMode = Summaries[StatsComboBox.SelectedIndex];
                    foreach (SummaryAggStat stat in GameMode.AggregatedStats.Stats)
                    {
                        ProfilePage.KeyValueItem item = new ProfilePage.KeyValueItem
                        {
                            Key = TitleCaseString(stat.StatType.Replace('_', ' ')),
                            Value = stat.Value.ToString()
                        };
                        StatsListView.Items.Add(item);
                    }

                    //Resize columns
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
                }));
            }
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