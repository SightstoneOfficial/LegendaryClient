using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Windows;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;
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
            if (Client.TrueCurrentPage.GetType() == typeof(ChampSelectPage))
            {
                try
                {
                    if (stats == null)
                    {

                        stats = new PlayerStatisticsChampSelect();
                        PublicSummoner summoner = await Client.PVPNet.GetSummonerByName(PlayerName.Content.ToString());
                        ChampionStatInfo[] TopChampions = await Client.PVPNet.RetrieveTopPlayedChampions(summoner.AcctId, "CLASSIC");
                        if (TopChampions[1] != null)
                            stats.MostPlayed.Source = champions.GetChampion((int)TopChampions[1].ChampionId).icon;
                        if (TopChampions[1] != null)
                            stats.Champion1.Content = champions.GetChampion((int)TopChampions[1].ChampionId).displayName + " - Games: " + TopChampions[1].TotalGamesPlayed;
                        if (TopChampions[2] != null)
                            stats.Champion2.Content = champions.GetChampion((int)TopChampions[2].ChampionId).displayName + " - Games: " + TopChampions[2].TotalGamesPlayed;
                        if (TopChampions[3] != null)
                            stats.Champion3.Content = champions.GetChampion((int)TopChampions[3].ChampionId).displayName + " - Games: " + TopChampions[3].TotalGamesPlayed;
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