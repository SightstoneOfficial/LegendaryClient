using System.Windows;
using Sightstone.Logic;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for ScreenshotViewer.xaml
    /// </summary>
    public partial class ScreenshotViewer
    {
        public ScreenshotViewer()
        {
            InitializeComponent();
        }
        private void CloseScreenshotsVierwerButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}