using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for CustomGameLobbyPage.xaml
    /// </summary>
    public partial class CustomGameLobbyPage : Page
    {
        private bool LaunchedTeamSelect = false;
        private bool IsOwner;
        private double OptomisticLock;

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

        private void GameLobby_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                GameDTO dto = message as GameDTO;
                if (dto.GameState == "TEAM_SELECT")
                {
                    OptomisticLock = dto.OptimisticLock;
                    LaunchedTeamSelect = false;
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                    {
                        BlueTeamListView.Items.Clear();
                        PurpleTeamListView.Items.Clear();

                        List<Participant> AllParticipants = new List<Participant>(dto.TeamOne.ToArray());
                        AllParticipants.AddRange(dto.TeamTwo);

                        int i = 0;
                        bool PurpleSide = false;

                        foreach (Participant playerTeam in AllParticipants)
                        {
                            i++;
                            CustomLobbyPlayer lobbyPlayer = new CustomLobbyPlayer();
                            if (playerTeam is PlayerParticipant)
                            {
                                PlayerParticipant player = playerTeam as PlayerParticipant;
                                lobbyPlayer = RenderPlayer(player, dto.OwnerSummary.SummonerId == player.SummonerId);
                                IsOwner = dto.OwnerSummary.SummonerId == Client.LoginPacket.AllSummonerData.Summoner.SumId;
                                StartGameButton.IsEnabled = IsOwner;

                                if (Client.Whitelist.Count > 0)
                                {
                                    if (!Client.Whitelist.Contains(player.SummonerName.ToLower()))
                                    {
                                        await Client.PVPNet.BanUserFromGame(Client.GameID, player.AccountId);
                                    }
                                }
                            }

                            if (i > dto.TeamOne.Count)
                            {
                                i = 0;
                                PurpleSide = true;
                            }

                            if (!PurpleSide)
                            {
                                BlueTeamListView.Items.Add(lobbyPlayer);
                            }
                            else
                            {
                                PurpleTeamListView.Items.Add(lobbyPlayer);
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
                            Client.LastPageContent = Client.Container.Content;
                            Client.SwitchPage(new ChampSelectPage());
                        }));
                        LaunchedTeamSelect = true;
                    }
                }
            }
        }

        private CustomLobbyPlayer RenderPlayer(PlayerParticipant player, bool IsOwner)
        {
            CustomLobbyPlayer lobbyPlayer = new CustomLobbyPlayer();
            lobbyPlayer.PlayerName.Content = player.SummonerName;

            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", player.ProfileIconId + ".png"), UriKind.RelativeOrAbsolute);
            lobbyPlayer.ProfileImage.Source = new BitmapImage(uriSource);

            if (IsOwner)
                lobbyPlayer.OwnerLabel.Visibility = Visibility.Visible;
            lobbyPlayer.Width = 400;
            lobbyPlayer.Margin = new Thickness(0, 0, 0, 5);
            if ((player.SummonerId == Client.LoginPacket.AllSummonerData.Summoner.SumId) ||
                (player.SummonerId != Client.LoginPacket.AllSummonerData.Summoner.SumId && !this.IsOwner))
            {
                lobbyPlayer.BanButton.Visibility = Visibility.Hidden;
            }
            lobbyPlayer.BanButton.Tag = player;
            lobbyPlayer.BanButton.Click += KickAndBan_Click;
            return lobbyPlayer;
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
            await Client.PVPNet.StartChampionSelection(Client.GameID, OptomisticLock);
        }
    }

    public class PlayerItem
    {
        public string Username { get; set; }

        public PlayerParticipant Participant { get; set; }
    }
}