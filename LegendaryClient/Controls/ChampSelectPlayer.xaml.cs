using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for ChampSelectPlayer.xaml
    /// </summary>
    public partial class ChampSelectPlayer : UserControl
    {
        private PlayerStatisticsChampSelect stats;

        public ChampSelectPlayer()
        {
            InitializeComponent();
        }

        private async void ChampPlayer_MouseOver(object sender, MouseEventArgs e)
        {
            if (Client.TrueCurrentPage.GetType() == typeof(ChampSelectPage) && !PlayerName.Content.ToString().StartsWith("Summoner ")) //Need to check if on your team incase you have a teammate such as Summoner I am a summoner... etc
            {
                try
                {
                    if (stats == null)
                    {
                        stats = new PlayerStatisticsChampSelect();
                        PublicSummoner summoner = await Client.PVPNet.GetSummonerByName(PlayerName.Content.ToString());
                        if (summoner == null || stats == null)
                        {
                            stats = null;
                            return;
                        }
                        ChampionStatInfo[] TopChampions = await Client.PVPNet.RetrieveTopPlayedChampions(summoner.AcctId, "CLASSIC");
                        stats.PlayerName.Content = summoner.Name;

                        if (TopChampions.Length > 0)
                        {
                            stats.MostPlayed.Source = champions.GetChampion((int)TopChampions[0].ChampionId).icon;
                            stats.Champion1.Content = champions.GetChampion((int)TopChampions[0].ChampionId).displayName + " - Games: " + TopChampions[0].TotalGamesPlayed;
                            double wins = 0.0;
                            double total = 0.0;
                            foreach (var stat in TopChampions[0].Stats)
                            {
                                if (stat.StatType == "TOTAL_SESSIONS_WON")
                                    wins = stat.Value;
                                else if (stat.StatType == "TOTAL_SESSIONS_PLAYED")
                                    total = stat.Value;
                            }

                            if ((wins / total * 100.0 != 0) && total != 0.0) stats.Champ1ProgressBar.Value = wins / total * 100.0;
                            else stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            stats.Champ1ProgressBar.Visibility = Visibility.Hidden;
                            stats.Champion1.Visibility = Visibility.Hidden;
                        }

                        if (TopChampions.Length > 1)
                        {
                            stats.Champion2.Content = champions.GetChampion((int)TopChampions[1].ChampionId).displayName + " - Games: " + TopChampions[1].TotalGamesPlayed;
                            double wins = 0.0;
                            double total = 0.0;
                            foreach (var stat in TopChampions[1].Stats)
                                if (stat.StatType == "TOTAL_SESSIONS_WON")
                                    wins = stat.Value;
                                else if (stat.StatType == "TOTAL_SESSIONS_PLAYED")
                                    total = stat.Value;

                            if ((wins / total * 100.0 != 0) && total != 0.0) stats.Champ2ProgressBar.Value = wins / total * 100.0;
                            else stats.Champ2ProgressBar.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            stats.Champ2ProgressBar.Visibility = Visibility.Hidden;
                            stats.Champion2.Visibility = Visibility.Hidden;
                        }

                        if (TopChampions.Length > 2)
                        {
                            stats.Champion3.Content = champions.GetChampion((int)TopChampions[2].ChampionId).displayName + " - Games: " + TopChampions[2].TotalGamesPlayed;
                            double wins = 0.0;
                            double total = 0.0;
                            foreach (var stat in TopChampions[2].Stats)
                                if (stat.StatType == "TOTAL_SESSIONS_WON")
                                    wins = stat.Value;
                                else if (stat.StatType == "TOTAL_SESSIONS_PLAYED")
                                    total = stat.Value;

                            if ((wins / total * 100.0 != 0) && total != 0.0) stats.Champ3ProgressBar.Value = wins / total * 100.0;
                            else stats.Champ3ProgressBar.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            stats.Champ3ProgressBar.Visibility = Visibility.Hidden;
                            stats.Champion3.Visibility = Visibility.Hidden;
                        }
                        Client.MainGrid.Children.Add(stats);
                    }
                    Point MouseLocation = e.GetPosition(Client.MainGrid);
                    double YMargin = MouseLocation.Y - 25;
                    if (Mouse.GetPosition(Client.MainGrid).X < 200)
                    {
                        stats.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        stats.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        stats.Margin = new Thickness(142, YMargin, 0, 0);
                    }
                    else
                    {
                        stats.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                        stats.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                        stats.Margin = new Thickness(0, YMargin, 155, 0);
                    }
                }
                catch { }
            }
        }


        private void ChampPlayer_MouseLeave(object sender, MouseEventArgs e)
        {
            if (stats != null)
            {
                Client.MainGrid.Children.Remove(stats);
                stats = null;
            }
        }
    }
}