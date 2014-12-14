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
            Change();

            Container.Content = new Masteries().Content;
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}