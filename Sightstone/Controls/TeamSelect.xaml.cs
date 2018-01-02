﻿using Sightstone.Logic;
using Sightstone.Windows;
using System.Windows;
using System.Collections.Generic;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.Riot.Team;
using Sightstone.Logic.MultiUser;


namespace Sightstone.Controls
{
    /// <summary>
    ///     Interaction logic for Warning.xaml
    /// </summary>
    public partial class TeamSelect
    {
        List<TeamId> TeamIdList = new List<TeamId>();
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
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