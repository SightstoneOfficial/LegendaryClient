#region

using System;
using System.Windows;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using LegendaryClient.Windows.Profile;

#endregion

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