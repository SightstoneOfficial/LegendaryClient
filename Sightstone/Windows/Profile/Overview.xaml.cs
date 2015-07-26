using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows.Profile
{
    /// <summary>
    ///     Interaction logic for Overview.xaml
    /// </summary>
    public partial class Overview
    {
        private double accId;
        private List<PlayerStatSummary> summaries = new List<PlayerStatSummary>();
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];

        public Overview()
        {
            InitializeComponent();
        }

        public async void Update(double summonerId, double accountId)
        {
            accId = accountId;
            var totalKudos =
                await UserClient.calls.CallKudos("{\"commandName\":\"TOTALS\",\"summonerId\": " + summonerId + "}");
            RenderKudos(totalKudos);
            var topChampions = await UserClient.calls.RetrieveTopPlayedChampions(accountId, "CLASSIC");
            RenderTopPlayedChampions(topChampions);
            GotPlayerStats(await UserClient.calls.RetrievePlayerStatsByAccountId(accountId, UserClient.LoginPacket.ClientSystemStates.currentSeason.ToString()));
        }

        public void RenderKudos(LcdsResponseString totalKudos)
        {
            KudosListView.Items.Clear();
            totalKudos.Value = totalKudos.Value.Replace("{\"totals\":[0,", string.Empty).Replace("]}", string.Empty);
            var kudos = totalKudos.Value.Split(',');
            var item = new KudosItem("Friendly", kudos[0]) {Height = 52};
            KudosListView.Items.Add(item);
            item = new KudosItem("Helpful", kudos[1]) {Height = 52};
            KudosListView.Items.Add(item);
            item = new KudosItem("Teamwork", kudos[2]) {Height = 52};
            KudosListView.Items.Add(item);
            item = new KudosItem("Honorable Opponent", kudos[3]) {Height = 52};
            KudosListView.Items.Add(item);
        }

        public void RenderTopPlayedChampions(ChampionStatInfo[] topChampions)
        {
            ViewAggregatedStatsButton.IsEnabled = false;
            TopChampionsListView.Items.Clear();
            if (!topChampions.Any())
            {
                return;
            }

            TopChampionsLabel.Content = "Top Champions (" + topChampions[0].TotalGamesPlayed + " Ranked Games)";
            foreach (var info in topChampions)
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
                    PlayerStatus = {Content = info.TotalGamesPlayed + " games played"},
                    ProfileImage = {Source = champions.GetChampion(champion.id).icon},
                    Background = new SolidColorBrush(Color.FromArgb(102, 80, 80, 80)),
                    Height = 52,
                    Width = 278
                };
                TopChampionsListView.Items.Add(player);
            }
        }

        public void GotPlayerStats(PlayerLifetimeStats stats)
        {
            summaries = new List<PlayerStatSummary>();
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                StatsComboBox.Items.Clear();
                StatsListView.Items.Clear();
                try
                {
                    foreach (
                        var x in
                            stats.PlayerStatSummaries.PlayerStatSummarySet.Where(x => x.AggregatedStats.Stats.Count > 0)
                        )
                    {
                        summaries.Add(x);
                        var summaryString = x.PlayerStatSummaryTypeString;
                        summaryString =
                            string.Concat(
                                summaryString.Select(
                                    e => Char.IsUpper(e) ? " " + e : e.ToString(CultureInfo.InvariantCulture)))
                                .TrimStart(' ');
                        summaryString = summaryString.Replace("Odin", "Dominion");
                        summaryString = summaryString.Replace("x", "v");
                        StatsComboBox.Items.Add(summaryString);
                    }
                }
                catch
                {
                    Client.Log("Error when loading player stats.");
                }
                if (StatsComboBox.Items.Contains("Ranked Solo5v5"))
                    StatsComboBox.SelectedItem = "Ranked Solo5v5";
                else
                    StatsComboBox.SelectedItem = "Unranked";
            }));
        }

        private void StatsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatsComboBox.SelectedIndex != -1)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    StatsListView.Items.Clear();
                    var gameMode = summaries[StatsComboBox.SelectedIndex];
                    foreach (
                        var Item in gameMode.AggregatedStats.Stats.Select(stat => new ProfilePage.KeyValueItem
                        {
                            Key = Client.TitleCaseString(stat.StatType.Replace('_', ' ')),
                            Value = stat.Value.ToString(CultureInfo.InvariantCulture)
                        })
                            .Select(
                                item =>
                                    new KudosItem(item.Key.ToString(), item.Value.ToString())
                                    {
                                        MinHeight = 18
                                    }))
                    {
                        Item.TypeLabel.FontSize = 12;
                        Item.AmountLabel.FontSize = 13;
                        StatsListView.Items.Add(Item);
                    }
                }));
            }
        }

        private async void ViewAggregatedStatsButton_Click(object sender, RoutedEventArgs e)
        {
            var x = await UserClient.calls.GetAggregatedStats(accId, "CLASSIC", UserClient.LoginPacket.ClientSystemStates.currentSeason.ToString());
            Client.OverlayContainer.Content =
                new AggregatedStatsOverlay(x, accId == UserClient.LoginPacket.AllSummonerData.Summoner.AcctId).Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }
    }
}