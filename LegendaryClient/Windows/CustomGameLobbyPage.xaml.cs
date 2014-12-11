using jabber.connection;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Logic.SQLite;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for CustomGameLobbyPage.xaml
    /// </summary>
    public partial class CustomGameLobbyPage : Page
    {
        private bool LaunchedTeamSelect;
        private bool IsOwner;
        private double OptomisticLock;
        private bool HasConnectedToChat;
        private Room newRoom;

        public CustomGameLobbyPage(GameDTO gameLobby = null)
        {
            InitializeComponent();

            GameName.Content = Client.GameName;
            Client.PVPNet.OnMessageReceived += GameLobby_OnMessageReceived;
            //If client has created game use initial DTO
            if (Client.GameLobbyDTO != null)
            {
                GameLobby_OnMessageReceived(null, Client.GameLobbyDTO);
            }
            else
            {
                Client.GameLobbyDTO = gameLobby;
                GameLobby_OnMessageReceived(null, Client.GameLobbyDTO);
            }
            Client.InviteListView = InviteListView;
            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Custom Game Lobby";
        }

        private void GameLobby_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                GameDTO dto = message as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    if (!HasConnectedToChat)
                    {
                        //Run once
                        BaseMap map = BaseMap.GetMap(dto.MapId);
                        MapLabel.Content = map.DisplayName;
                        ModeLabel.Content = Client.TitleCaseString(dto.GameMode);
                        GameTypeConfigDTO configType = Client.LoginPacket.GameTypeConfigs.Find(x => x.Id == dto.GameTypeConfigId);
                        TypeLabel.Content = GetGameMode(configType.Id);
                        SizeLabel.Content = dto.MaxNumPlayers / 2 + "v" + dto.MaxNumPlayers / 2;

                        HasConnectedToChat = true;
                        try
                        {
                            string ObfuscatedName = Client.GetObfuscatedChatroomName(dto.Name.ToLower() + Convert.ToInt32(dto.Id), ChatPrefixes.Arranging_Practice);
                            string JID = Client.GetChatroomJID(ObfuscatedName, dto.RoomPassword, false);
                            newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
                            newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
                            newRoom.OnRoomMessage += newRoom_OnRoomMessage;
                            newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
                            newRoom.Join(dto.RoomPassword);
                        }
                        catch { }
                    }
                    if (dto.GameState == "TEAM_SELECT")
                    {
                        OptomisticLock = dto.OptimisticLock;
                        LaunchedTeamSelect = false;
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
                            BotControl botPlayer = new BotControl();
                            if (playerTeam is PlayerParticipant)
                            {
                                PlayerParticipant player = playerTeam as PlayerParticipant;
                                lobbyPlayer = RenderPlayer(player, dto.OwnerSummary.SummonerId == player.SummonerId);
                                IsOwner = dto.OwnerSummary.SummonerId == Client.LoginPacket.AllSummonerData.Summoner.SumId;
                                StartGameButton.IsEnabled = IsOwner;
                                AddBotBlueTeam.IsEnabled = IsOwner;
                                AddBotPurpleTeam.IsEnabled = IsOwner;

                                if (Client.Whitelist.Count > 0)
                                {
                                    if (!Client.Whitelist.Contains(player.SummonerName.ToLower()))
                                    {
                                        await Client.PVPNet.BanUserFromGame(Client.GameID, player.AccountId);
                                    }
                                }
                            }
                            else if (playerTeam is BotParticipant)
                            {
                                BotParticipant botParticipant = playerTeam as BotParticipant;
                                botPlayer = RenderBot(botParticipant);
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
                    }
                    else if (dto.GameState == "CHAMP_SELECT" || dto.GameState == "PRE_CHAMP_SELECT")
                    {
                        if (!LaunchedTeamSelect)
                        {
                            Client.ChampSelectDTO = dto;
                            Client.LastPageContent = Client.Container.Content;
                            Client.SwitchPage(new ChampSelectPage(this));
                            Client.GameStatus = "championSelect";
                            Client.SetChatHover();
                            LaunchedTeamSelect = true;
                        }
                    }
                }));
            }
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            }));
        }

        public void Invite_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new InvitePlayersPage().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void newRoom_OnRoomMessage(object sender, jabber.protocol.client.Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (msg.Body != "This room is not anonymous")
                {
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.From.Resource + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    if (Client.Filter)
                        tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "").Filter() + Environment.NewLine;
                    else
                        tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }
            }));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            if (Client.Filter)
                tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
            else
                tr.Text = ChatTextBox.Text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            newRoom.PublicMessage(ChatTextBox.Text);
            ChatTextBox.Text = "";
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

        private BotControl RenderBot(BotParticipant BotPlayer)
        {
            BotControl botPlayer = new BotControl();
            botPlayer.PlayerName.Content = BotPlayer.SummonerName;
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", BotPlayer.Champion + ".png"), UriKind.RelativeOrAbsolute);
            botPlayer.ProfileImage.Source = new BitmapImage(uriSource);
            botPlayer.BanButton.Tag = BotPlayer;
            botPlayer.BanButton.Click += KickAndBan_Click;
            return botPlayer;
        }

        private async void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            await Client.PVPNet.QuitGame();
            Client.ClearPage(typeof(CustomGameLobbyPage)); //Clear pages
            Client.ClearPage(typeof(CreateCustomGamePage));
            Client.ReturnButton.Visibility = Visibility.Hidden;
            uiLogic.UpdateMainPage();
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

        public static string GetGameMode(int i)
        {
            switch (i)
            {
                case 1:
                    return "Blind Pick";

                case 3:
                    return "No Ban Draft";

                case 4:
                    return "All Random";

                case 5:
                    return "Open Pick";

                case 6:
                    return "Tournament Draft";

                case 7:
                    return "Blind Draft";

                case 11:
                    return "Infinite Time Blind Pick";

                case 12:
                    return "Captain Pick";
                    //R.I.P One for All
                    /*
                case 14:
                    return "One for All";*/

                default:
                    return Client.LoginPacket.GameTypeConfigs.Find(x => x.Id == i).Name;
            }
        }

        private static List<Int32> bots = new List<Int32>(new Int32[] 
        { 
            12, 32, 1, 22, 53, 63, 51, 69, 31, 42,
            122, 36, 81, 9, 3, 86, 104, 39, 59, 24,
            30, 10, 96, 89, 236, 99, 54, 90, 11, 21,
            62, 25, 75, 20, 33, 58, 13, 98, 102, 15,
            16, 50, 44, 18, 48, 77, 45, 8, 19, 5,
            115, 26, 143
        });

        private Int32 getRandomChampInt()
        {
            Random rnd = new Random();
            Int32 r = rnd.Next(bots.Count);
            return bots[r];
        }

        private async void AddBotBlueTeam_Click(object sender, RoutedEventArgs e)
        {
            Int32 champint = getRandomChampInt();
            champions champions = champions.GetChampion(champint);
            ChampionDTO champDTO = new ChampionDTO();
            champDTO.Active = true;
            champDTO.Banned = false;
            champDTO.BotEnabled = true;
            champDTO.ChampionId = champint;
            champDTO.DisplayName = champions.displayName;

            List<ChampionSkinDTO> skinlist = new List<ChampionSkinDTO>();
            foreach (Dictionary<string, object> Skins in champions.Skins)
            {
                ChampionSkinDTO skin = new ChampionSkinDTO();
                skin.ChampionId = champint;
                skin.FreeToPlayReward = false;
                Int32 SkinInt = Convert.ToInt32(Skins["id"]);
                skin.SkinId = SkinInt;
                List<ChampionDTO> champs = new List<ChampionDTO>(Client.PlayerChampions);
                foreach (ChampionDTO x in champs)
                {
                    foreach (ChampionSkinDTO myskin in x.ChampionSkins)
                    {
                        if (myskin.Owned) { }
                    }
                }
                skin.Owned = true;
                skinlist.Add(skin);
            }
            champDTO.ChampionSkins = skinlist;

            BotParticipant par = new BotParticipant();
            par.BotSkillLevelName = "Basic";
            par.BotSkillLevel = 0;
            par.Champion = champDTO;

            await Client.PVPNet.SelectBotChampion(champint, par);


            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                
                
            }));
        }

        private void AddBotPurpleTeam_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                //Needs looked into
            }));
        }
    }
}