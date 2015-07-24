using Sightstone.Logic;
using Sightstone.Logic.MultiUser;
using Sightstone.Windows.Profile;
using System.Windows;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for RunesOverlay.xaml
    /// </summary>
    public partial class RunesOverlay
    {
        public RunesOverlay()
        {
            InitializeComponent();
            Container.Content = new Runes().Content;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}