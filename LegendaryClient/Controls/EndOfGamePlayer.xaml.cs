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
        public EndOfGamePlayer()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var reportPlayer = new ReportPlayerOverlay();
            Client.OverOverlayContainer.Content = reportPlayer.Content;
            Client.OverOverlayContainer.Visibility = Visibility.Visible;
        }
    }
}