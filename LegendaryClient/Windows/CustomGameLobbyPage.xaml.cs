using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Team;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for CustomGameLobbyPage.xaml
    /// </summary>
    public partial class CustomGameLobbyPage : Page
    {
        bool LaunchedTeamSelect = false;

        public CustomGameLobbyPage()
        {
            InitializeComponent();

            GameName.Content = Client.GameName;
            Client.PVPNet.OnMessageReceived += GameLobby_OnMessageReceived;
            //If client has created game use initial DTO
            if (Client.GameLobbyDTO != null)
            {
                GameLobby_OnMessageReceived(null, Client.GameLobbyDTO);
            }
        }

        void GameLobby_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                GameDTO dto = message as GameDTO;
                if (dto.GameState == "TEAM_SELECT")
                {
                    LaunchedTeamSelect = false;
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                    {
                        BlueTeamListView.Items.Clear();
                        PurpleTeamListView.Items.Clear();

                        //Update ListBoxes
                        foreach (Participant playerTeam1 in dto.TeamOne)
                        {
                            try
                            {
                                PlayerParticipant player = playerTeam1 as PlayerParticipant;
                                BlueTeamListView.Items.Add(
                                    new PlayerItem { Username = player.SummonerName, Participant = player});
                                if (Client.Whitelist.Count > 0)
                                {
                                    if (!Client.Whitelist.Contains(player.SummonerName.ToLower()))
                                    {
                                        await Client.PVPNet.BanUserFromGame(Client.GameID, player.AccountId);
                                    }
                                }
                            }
                            catch
                            {
                                //Robert
                            }
                        }
                        foreach (Participant playerTeam2 in dto.TeamTwo)
                        {
                            try
                            {
                                PlayerParticipant player = playerTeam2 as PlayerParticipant;
                                PurpleTeamListView.Items.Add(
                                    new PlayerItem { Username = player.SummonerName, Participant = player});
                                if (Client.Whitelist.Count > 0)
                                {
                                    if (!Client.Whitelist.Contains(player.SummonerName.ToLower()))
                                    {
                                        await Client.PVPNet.BanUserFromGame(Client.GameID, player.AccountId);
                                    }
                                }
                            }
                            catch
                            {
                                //Robert
                            }
                        }
                    }));
                }
                else if (dto.GameState == "CHAMP_SELECT" || dto.GameState == "PRE_CHAMP_SELECT")
                {
                    if (!LaunchedTeamSelect)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            Client.ChampSelectDTO = dto;
                            Client.SwitchPage(new ChampSelectPage());
                        }));
                        LaunchedTeamSelect = true;
                    }
                }
            }
        }

        private async void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            await Client.PVPNet.QuitGame();
            Client.ClearPage(new CustomGameLobbyPage()); //Clear pages
            Client.ClearPage(new CreateCustomGamePage());

            Client.SwitchPage(new MainPage());
        }

        private async void SwitchTeamsButton_Click(object sender, RoutedEventArgs e)
        {
            await Client.PVPNet.SwitchTeams(Client.GameID);   
        }

        private async void KickAndBan_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            PlayerParticipant BanPlayer = (PlayerParticipant)button.Tag;
            await Client.PVPNet.BanUserFromGame(Client.GameID, BanPlayer.AccountId);
        }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            await Client.PVPNet.StartChampionSelection(Client.GameID, BlueTeamListView.Items.Count + PurpleTeamListView.Items.Count);
        }
    }

    public class PlayerItem
    {
        public string Username { get; set; }

        public PlayerParticipant Participant { get; set; }
    }
}
