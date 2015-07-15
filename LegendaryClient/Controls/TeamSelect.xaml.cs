using LegendaryClient.Logic;
using LegendaryClient.Windows;
using System.Windows;
using System.Collections.Generic;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform;
using LegendaryClient.Logic.Riot.Team;
using LegendaryClient.Logic.MultiUser;


namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for Warning.xaml
    /// </summary>
    public partial class TeamSelect
    {
        List<TeamId> TeamIdList = new List<TeamId>();
        static UserClient UserClient = UserList.Users[Client.Current];
        public TeamSelect()
        {
            InitializeComponent();
            GetTeams();
        }
        private async void GetTeams()
        {
            PlayerDTO allTeams = await UserClient.calls.FindPlayer(UserClient.LoginPacket.AllSummonerData.Summoner.SumId);
            if (allTeams.TeamsSummary != null)
            {
                foreach (var item in allTeams.TeamsSummary)
                {
                    TeamDTO teams = (TeamDTO)item;
                    TeamList.Items.Add(teams.Name);
                    TeamIdList.Add(teams.TeamId);
                }
            }
            else
            {
                Header.Content = "Join a team first";
                SelectTeam.IsEnabled = false;
            }
        }

        private async void SelectTeam_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LobbyStatus lobby = await UserClient.calls.CreateArrangedRankedTeamLobby(UserClient.QueueId, TeamList.SelectedItem.ToString());

            TeamId SelectedTeamId = TeamIdList[TeamList.SelectedIndex];
            Client.ClearPage(typeof(TeamQueuePage));
            Client.SwitchPage(new TeamQueuePage(lobby.InvitationID, lobby, false, SelectedTeamId));
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
        }
    }
}