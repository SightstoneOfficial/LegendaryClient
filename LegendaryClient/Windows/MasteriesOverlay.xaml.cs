using LegendaryClient.Logic;
using LegendaryClient.Windows.Profile;
using System.Windows;
using System.Windows.Controls;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for MasteriesOverlay.xaml
    /// </summary>
    public partial class MasteriesOverlay : Page
    {
        public MasteriesOverlay()
        {
            InitializeComponent();
            Container.Content = new Masteries().Content;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}