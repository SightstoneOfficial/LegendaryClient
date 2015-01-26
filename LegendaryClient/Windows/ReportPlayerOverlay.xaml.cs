#region

using System;
using System.Windows;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Harassment;
using System.Windows.Documents;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ReportPlayerOverlay.xaml
    /// </summary>
    public partial class ReportPlayerOverlay
    {
        public ReportPlayerOverlay()
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

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverOverlayContainer.Visibility = Visibility.Hidden;
        }

        private async void SendReport_Click(object sender, RoutedEventArgs e)
        {
            TextRange text = new TextRange(ReportText.Document.ContentStart, ReportText.Document.ContentEnd);
            HarassmentReport report = new HarassmentReport()
            {
                GameID = 125,
                Offense = "UNSKILLED_PLAYER",
                ReportingSummonerID = 123,
                ReportSource = "GAME",
                SummonerID = 135,
                Comment = text.Text
            };
            await Client.PVPNet.ReportPlayer(report);
        }
    }
}