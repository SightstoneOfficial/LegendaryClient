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

        public EndOfGamePlayer(double summonerID, double gameID, string summonerName)
        {
            InitializeComponent();
            this.summonerID = summonerID;
            this.gameID = gameID;
            this.summonerName = summonerName;
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var reportPlayer = new ReportPlayerOverlay(summonerID, gameID, summonerName, ReportButton);
            Client.OverOverlayContainer.Content = reportPlayer.Content;
            Client.OverOverlayContainer.Visibility = Visibility.Visible;
        }
    }
}