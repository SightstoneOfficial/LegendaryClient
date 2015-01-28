using LegendaryClient.Logic;
using LegendaryClient.Windows;
using PVPNetConnect.RiotObjects.Team.Dto;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using PVPNetConnect;
using System.Windows;


namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for Warning.xaml
    /// </summary>
    public partial class TeamSelect
    {
        public TeamSelect()
        {
            InitializeComponent();
            GetTeams();
        }
        private async void GetTeams()
        {
            PlayerDTO asd = await Client.PVPNet.FindPlayer(Client.LoginPacket.AllSummonerData.Summoner.SumId);
            foreach (var item in asd.TeamsSummary)
            {
                TeamDTO qwe = new PVPNetConnect.RiotObjects.Team.Dto.TeamDTO((TypedObject)item);
                TeamList.Items.Add(qwe.Name);
            }
        }

        private async void SelectTeam_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LobbyStatus lobby = await Client.PVPNet.createArrangedRankedTeamLobby(Client.QueueId, TeamList.SelectedItem.ToString());

            Client.ClearPage(typeof(TeamQueuePage));
            Client.SwitchPage(new TeamQueuePage(lobby.InvitationID, lobby));
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}