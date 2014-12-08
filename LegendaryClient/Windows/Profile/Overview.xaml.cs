﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Harassment;
using PVPNetConnect.RiotObjects.Platform.Statistics;

namespace LegendaryClient.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview
    {
        private double AccId;
        private List<PlayerStatSummary> Summaries = new List<PlayerStatSummary>();

        public Overview()
        {
            InitializeComponent();
        }

        public async void Update(double SummonerId, double AccountId)
        {
            AccId = AccountId;
            LcdsResponseString TotalKudos =
                await Client.PVPNet.CallKudos("{\"commandName\":\"TOTALS\",\"summonerId\": " + SummonerId + "}");
            RenderKudos(TotalKudos);
            ChampionStatInfo[] TopChampions = await Client.PVPNet.RetrieveTopPlayedChampions(AccountId, "CLASSIC");
            RenderTopPlayedChampions(TopChampions);
            Client.PVPNet.RetrievePlayerStatsByAccountId(AccountId, "3", GotPlayerStats);
        }

        public void RenderKudos(LcdsResponseString TotalKudos)
        {
            KudosListView.Items.Clear();
            TotalKudos.Value = TotalKudos.Value.Replace("{\"totals\":[0,", "").Replace("]}", "");
            string[] Kudos = TotalKudos.Value.Split(',');
            var item = new KudosItem("Friendly", Kudos[0]);
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
            ViewAggregatedStatsButton.IsEnabled = false;
            TopChampionsListView.Items.Clear();
            if (!TopChampions.Any())
                return;

            TopChampionsLabel.Content = "Top Champions (" + TopChampions[0].TotalGamesPlayed + " Ranked Games)";
            foreach (ChampionStatInfo info in TopChampions)
            {
                ViewAggregatedStatsButton.IsEnabled = true;
                if (!(Math.Abs(info.ChampionId) > 0))
                    continue;

                var player = new ChatPlayer();
                champions Champion = champions.GetChampion(Convert.ToInt32(info.ChampionId));
                player.LevelLabel.Visibility = Visibility.Hidden;
                player.PlayerName.Content = Champion.displayName;
                player.PlayerStatus.Content = info.TotalGamesPlayed + " games played";
                player.ProfileImage.Source = champions.GetChampion(Champion.id).icon;
                player.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(102, 80, 80, 80));
                player.Width = 270;
                TopChampionsListView.Items.Add(player);
            }
        }

        public void GotPlayerStats(PlayerLifetimeStats stats)
        {
            Summaries = new List<PlayerStatSummary>();
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                StatsComboBox.Items.Clear();
                StatsListView.Items.Clear();
                try
                {
                    foreach (
                        PlayerStatSummary x in
                            stats.PlayerStatSummaries.PlayerStatSummarySet.Where(x => x.AggregatedStats.Stats.Count > 0)
                        )
                    {
                        Summaries.Add(x);
                        string SummaryString = x.PlayerStatSummaryTypeString;
                        SummaryString =
                            string.Concat(SummaryString.Select(e => Char.IsUpper(e) ? " " + e : e.ToString()))
                                .TrimStart(' ');
                        SummaryString = SummaryString.Replace("Odin", "Dominion");
                        SummaryString = SummaryString.Replace("x", "v");
                        StatsComboBox.Items.Add(SummaryString);
                    }
                }
                catch
                {
                    Client.Log("Error when loading player stats.");
                }
                StatsComboBox.SelectedItem = "Ranked Solo5v5";
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
                    foreach (
                        ProfilePage.KeyValueItem item in
                            GameMode.AggregatedStats.Stats.Select(stat => new ProfilePage.KeyValueItem
                            {
                                Key = Client.TitleCaseString(stat.StatType.Replace('_', ' ')),
                                Value = stat.Value.ToString(CultureInfo.InvariantCulture)
                            }))
                    {
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

        private async void ViewAggregatedStatsButton_Click(object sender, RoutedEventArgs e)
        {
            AggregatedStats x = await Client.PVPNet.GetAggregatedStats(AccId, "CLASSIC", "3");
            Client.OverlayContainer.Content =
                new AggregatedStatsOverlay(x, AccId == Client.LoginPacket.AllSummonerData.Summoner.AcctId).Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }
    }
}