using LegendaryClient.Logic;
using System.Windows;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for MessageOverlay.xaml
    /// </summary>
    public partial class MessageOverlay
    {
        public MessageOverlay()
        {
            InitializeComponent();
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}