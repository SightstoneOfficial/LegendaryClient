using LegendaryClient.Logic;
using LegendaryClient.Windows;
using System.Windows;
namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for EndOfGamePlayer.xaml
    /// </summary>
    public partial class EndOfGamePlayer
    {
        private double summonerID;
        private double gameID;
        private string summonerName;
        private bool sameTeam;

        public EndOfGamePlayer(double summonerID, double gameID, string summonerName, bool sameTeam)
        {
            InitializeComponent();
            this.summonerID = summonerID;
            this.gameID = gameID;
            this.summonerName = summonerName;
            this.sameTeam = sameTeam;
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var reportPlayer = new ReportPlayerOverlay(summonerID, gameID, summonerName, ReportButton);
            Client.OverOverlayContainer.Content = reportPlayer.Content;
            Client.OverOverlayContainer.Visibility = Visibility.Visible;
        }

        private void KudosButton_Click(object sender, RoutedEventArgs e)
        {

            var honorPlayer = new KudosPlayerOverlay(summonerID, gameID, summonerName, sameTeam);
            Client.OverOverlayContainer.Content = honorPlayer.Content;
            Client.OverOverlayContainer.Visibility = Visibility.Visible;
        }
    }
}