using LegendaryClient.Logic;
using LegendaryClient.Logic.MultiUser;
using LegendaryClient.Windows.Profile;
using System.Windows;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for MasteriesOverlay.xaml
    /// </summary>
    public partial class MasteriesOverlay
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