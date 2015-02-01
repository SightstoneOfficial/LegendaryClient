#region

using System;
using System.Windows;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Harassment;
using System.Windows.Documents;
using System.Windows.Controls;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for ReportPlayerOverlay.xaml
    /// </summary>
    public partial class KudosPlayerOverlay
    {
        private double summonerID, gameID;
        bool sameTeam;

        public KudosPlayerOverlay(double summonerID, double gameID, string summonerName, bool sameTeam)
        {
            InitializeComponent();
            Change();
            this.summonerID = summonerID;
            this.gameID = gameID;
            this.sameTeam = sameTeam;
            PlayerNameLabel.Content = summonerName;
            GetKudos();
        }

        private void GetKudos()
        {
            if(sameTeam)
            {
                KudosList.Items.Add("Friendly");
                KudosList.Items.Add("Helpful");
                KudosList.Items.Add("Teamwork");
            }
            else
            {
                KudosList.Items.Add("Honorable Opponent");
            }
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

        private async void GiveButton_Click(object sender, RoutedEventArgs e)
        {
            GiveButton.IsEnabled = false;
            if (sameTeam)
                await Client.PVPNet.CallKudos(
                    "{\"gameId\":" + gameID +
                    ",\"commandName\":\"GIVE\"" +
                    ",\"giverId\":" + Client.LoginPacket.AllSummonerData.Summoner.SumId +
                    ",\"kudosType\":" + KudosList.SelectedIndex++ + 
                    ",\"receiverId\": " + summonerID + "}");
            else
                await Client.PVPNet.CallKudos(
                    "{\"gameId\":" + gameID +
                    ",\"commandName\":\"GIVE\"" + 
                    ",\"giverId\":" + Client.LoginPacket.AllSummonerData.Summoner.SumId +
                    ",\"kudosType\":\"4\"" + 
                    ",\"receiverId\":" + summonerID + "}");

            Client.OverOverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}