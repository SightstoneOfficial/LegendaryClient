using Sightstone.Logic;
using System.Windows;
using Sightstone.Logic.Riot;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for ReportPlayerOverlay.xaml
    /// </summary>
    public partial class KudosPlayerOverlay
    {
        private double summonerID, gameID;
        bool sameTeam;
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
        public KudosPlayerOverlay(double summonerID, double gameID, string summonerName, bool sameTeam)
        {
            InitializeComponent();
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
                KudosList.Items.Add("Honorable Opponent");
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverOverlayContainer.Visibility = Visibility.Hidden;
        }

        private async void GiveButton_Click(object sender, RoutedEventArgs e)
        {
            int kudoId = KudosList.SelectedIndex + 1;
            GiveButton.IsEnabled = false;
            if (sameTeam)
                await UserClient.calls.CallKudos(
                    "{\"gameId\":" + gameID +
                    ",\"commandName\":\"GIVE\"" +
                    ",\"giverId\":" + UserClient.LoginPacket.AllSummonerData.Summoner.SumId +
                    ",\"kudosType\":" + kudoId + 
                    ",\"receiverId\": " + summonerID + "}");
            else
                await UserClient.calls.CallKudos(
                    "{\"gameId\":" + gameID +
                    ",\"commandName\":\"GIVE\"" + 
                    ",\"giverId\":" + UserClient.LoginPacket.AllSummonerData.Summoner.SumId +
                    ",\"kudosType\":\"4\"" + 
                    ",\"receiverId\":" + summonerID + "}");

            Client.OverOverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}