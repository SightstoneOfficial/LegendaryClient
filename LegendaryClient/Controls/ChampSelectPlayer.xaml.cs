#region

using System;
using System.Windows;
using System.Windows.Input;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for ChampSelectPlayer.xaml
    /// </summary>
    public partial class ChampSelectPlayer
    {
        private PlayerStatisticsChampSelect _stats;

        public ChampSelectPlayer()
        {
            InitializeComponent();
        }

        private async void ChampPlayer_MouseOver(object sender, MouseEventArgs e)
        {
            if (Client.TrueCurrentPage.GetType() != typeof (ChampSelectPage) ||
                PlayerName.Content.ToString().StartsWith("Summoner "))
                return;

            try
            {
                if (_stats == null)
                {
                    _stats = new PlayerStatisticsChampSelect();
                    PublicSummoner summoner = await Client.PVPNet.GetSummonerByName(PlayerName.Content.ToString());
                    if (summoner == null || _stats == null || summoner.InternalName.Contains("bot"))
                    {
                        _stats = null;
                        return;
                    }
                    ChampionStatInfo[] topChampions =
                        await Client.PVPNet.RetrieveTopPlayedChampions(summoner.AcctId, "CLASSIC");
                    _stats.PlayerName.Content = summoner.Name;

                    if (topChampions.Length > 0)
                    {
                        _stats.MostPlayed.Source = champions.GetChampion((int) topChampions[0].ChampionId).icon;
                        _stats.Champion1.Content =
                            champions.GetChampion((int) topChampions[0].ChampionId).displayName + " - Games: " +
                            topChampions[0].TotalGamesPlayed;
                        double wins = 0.0;
                        double total = 0.0;
                        foreach (AggregatedStat stat in topChampions[0].Stats)
                        {
                            switch (stat.StatType)
                            {
                                case "TOTAL_SESSIONS_WON":
                                    wins = stat.Value;
                                    break;
                                case "TOTAL_SESSIONS_PLAYED":
                                    total = stat.Value;
                                    break;
                            }
                        }

                        if ((Math.Abs(wins/total*100.0) > 0) && Math.Abs(total) > 0)
                            _stats.Champ1ProgressBar.Value = wins/total*100.0;
                        else _stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        _stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                        _stats.Champion1.Visibility = Visibility.Hidden;
                    }

                    if (topChampions.Length > 1)
                    {
                        _stats.Champion2.Content =
                            champions.GetChampion((int) topChampions[1].ChampionId).displayName + " - Games: " +
                            topChampions[1].TotalGamesPlayed;
                        double wins = 0.0;
                        double total = 0.0;
                        foreach (AggregatedStat stat in topChampions[1].Stats)
                            if (stat.StatType == "TOTAL_SESSIONS_WON")
                                wins = stat.Value;
                            else if (stat.StatType == "TOTAL_SESSIONS_PLAYED")
                                total = stat.Value;

                        if ((Math.Abs(wins/total*100.0) > 0) && Math.Abs(total) > 0)
                            _stats.Champ2ProgressBar.Value = wins/total*100.0;
                        else _stats.Champ2ProgressBar.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        _stats.Champ2ProgressBar.Visibility = Visibility.Hidden;
                        _stats.Champion2.Visibility = Visibility.Hidden;
                    }

                    if (topChampions.Length > 2)
                    {
                        _stats.Champion3.Content =
                            champions.GetChampion((int) topChampions[2].ChampionId).displayName + " - Games: " +
                            topChampions[2].TotalGamesPlayed;
                        double wins = 0.0;
                        double total = 0.0;
                        foreach (AggregatedStat stat in topChampions[2].Stats)
                            if (stat.StatType == "TOTAL_SESSIONS_WON")
                                wins = stat.Value;
                            else if (stat.StatType == "TOTAL_SESSIONS_PLAYED")
                                total = stat.Value;

                        if ((Math.Abs(wins/total*100.0) > 0) && Math.Abs(total) > 0)
                            _stats.Champ3ProgressBar.Value = wins/total*100.0;
                        else _stats.Champ3ProgressBar.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        _stats.Champ3ProgressBar.Visibility = Visibility.Hidden;
                        _stats.Champion3.Visibility = Visibility.Hidden;
                    }
                    Client.MainGrid.Children.Add(_stats);
                }
                Point mouseLocation = e.GetPosition(Client.MainGrid);
                double yMargin = mouseLocation.Y - 25;
                if (Mouse.GetPosition(Client.MainGrid).X < 200)
                {
                    _stats.HorizontalAlignment = HorizontalAlignment.Left;
                    _stats.VerticalAlignment = VerticalAlignment.Top;
                    _stats.Margin = new Thickness(142, yMargin, 0, 0);
                }
                else
                {
                    _stats.HorizontalAlignment = HorizontalAlignment.Right;
                    _stats.VerticalAlignment = VerticalAlignment.Top;
                    _stats.Margin = new Thickness(0, yMargin, 155, 0);
                }
            }
            catch (Exception)
            {
            }
        }


        private void ChampPlayer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_stats == null)
                return;

            Client.MainGrid.Children.Remove(_stats);
            _stats = null;
        }
    }
}