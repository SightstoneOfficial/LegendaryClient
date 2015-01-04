#region

using System;
using System.Windows;
using LegendaryClient.Logic;
using LegendaryClient.Properties;

#endregion

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
            Change();
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}