using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;


namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for EndOfGamePage.xaml
    /// </summary>
    public partial class EndOfGamePage : Page
    {
        public EndOfGamePage(EndOfGameStats Statistics)
        {
            InitializeComponent();
            RenderStats(Statistics);
        }

        private void RenderStats(EndOfGameStats Statistics)
        {
            TimeSpan t = TimeSpan.FromSeconds(Statistics.GameLength);
            TimeLabel.Content = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
            TypeLabel.Content = Statistics.GameMode;


            championSkins Skin = championSkins.GetSkin(Statistics.SkinIndex);
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Skin.splashPath), UriKind.Absolute);
            SkinImage.Source = new BitmapImage(uriSource);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}
