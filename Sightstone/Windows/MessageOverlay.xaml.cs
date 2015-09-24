using Sightstone.Logic;
using Sightstone.Logic.MultiUser;
using System.Windows;

namespace Sightstone.Windows
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