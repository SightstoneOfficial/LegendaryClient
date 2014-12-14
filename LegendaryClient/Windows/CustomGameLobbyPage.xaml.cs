#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using jabber;
using jabber.connection;
using jabber.protocol.client;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using PVPNetConnect.RiotObjects.Platform.Game;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for CustomGameLobbyPage.xaml
    /// </summary>
    public partial class CustomGameLobbyPage
    {
        private static readonly List<Int32> bots = new List<Int32>(new[]
        {
            12, 32, 1, 22, 53, 63, 51, 69, 31, 42,
            122, 36, 81, 9, 3, 86, 104, 39, 59, 24,
            30, 10, 96, 89, 236, 99, 54, 90, 11, 21,
            62, 25, 75, 20, 33, 58, 13, 98, 102, 15,
            16, 50, 44, 18, 48, 77, 45, 8, 19, 5,
            115, 26, 143
        });

        private bool HasConnectedToChat;

        private bool IsOwner;
        private bool LaunchedTeamSelect;
        private double OptomisticLock;
        private Room newRoom;

        public CustomGameLobbyPage(GameDTO gameLobby = null)
        {
            InitializeComponent();
            Change();

            GameName.Content = Client.GameName;
            Client.PVPNet.OnMessageReceived += GameLobby_OnMessageReceived;
            //If client has created game use initial DTO
            if (Client.GameLobbyDTO != null)
                GameLobby_OnMessageReceived(null, Client.GameLobbyDTO);
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

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void GameLobby_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() != typeof(GameDTO))
                return;

            var dto = message as GameDTO;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                if (!HasConnectedToChat)
                {
                    //Run once
                    BaseMap map = BaseMap.GetMap(dto.MapId);
                    MapLabel.Content = map.DisplayName;
                    ModeLabel.Content = Client.TitleCaseString(dto.GameMode);
                    GameTypeConfigDTO configType =
                        Client.LoginPacket.GameTypeConfigs.Find(x => x.Id == dto.GameTypeConfigId);
                    TypeLabel.Content = GetGameMode(configType.Id);
                    SizeLabel.Content = dto.MaxNumPlayers / 2 + "v" + dto.MaxNumPlayers / 2;

                    HasConnectedToChat = true;
                    try
                    {
                        string obfuscatedName =
                            Client.GetObfuscatedChatroomName(dto.Name.ToLower() + Convert.ToInt32(dto.Id),
                                ChatPrefixes.Arranging_Practice);
                        string jid = Client.GetChatroomJID(obfuscatedName, dto.RoomPassword, false);
                        newRoom = Client.ConfManager.GetRoom(new JID(jid));
                        newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
                        newRoom.OnRoomMessage += newRoom_OnRoomMessage;
                        newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
                        newRoom.Join(dto.RoomPassword);
                    }
                    catch
                    {
                    }
                }
                switch (dto.GameState)
                {
                    case "TEAM_SELECT":
                        {
                            OptomisticLock = dto.OptimisticLock;
                            LaunchedTeamSelect = false;
                            BlueTeamListView.Items.Clear();
                            PurpleTeamListView.Items.Clear();

                            foreach (Participant playerTeam in dto.TeamOne)
                            {
                                if (playerTeam is PlayerParticipant)
                                {
                                    var lobbyPlayer = new CustomLobbyPlayer();
                                    var player = playerTeam as PlayerParticipant;
                                    lobbyPlayer = RenderPlayer(player, dto.OwnerSummary.SummonerId == player.SummonerId);
                                    IsOwner = dto.OwnerSummary.SummonerId ==
                                              Client.LoginPacket.AllSummonerData.Summoner.SumId;
                                    StartGameButton.IsEnabled = IsOwner;
                                    AddBotBlueTeam.IsEnabled = IsOwner;
                                    AddBotPurpleTeam.IsEnabled = IsOwner;

                                    BlueTeamListView.Items.Add(lobbyPlayer);

                                    if (Client.Whitelist.Count > 0)
                                        if (!Client.Whitelist.Contains(player.SummonerName.ToLower()))
                                            await Client.PVPNet.BanUserFromGame(Client.GameID, player.AccountId);
                                }
                                else if (playerTeam is BotParticipant)
                                {
                                    var botParticipant = playerTeam as BotParticipant;
                                    var botPlayer = new BotControl();
                                    botPlayer = RenderBot(botParticipant);
                                    BlueTeamListView.Items.Add(botPlayer);
                                }
                            }
                            foreach (Participant playerTeam in dto.TeamTwo)
                            {
                                if (playerTeam is PlayerParticipant)
                                {
                                    var lobbyPlayer = new CustomLobbyPlayer();
                                    var player = playerTeam as PlayerParticipant;
                                    lobbyPlayer = RenderPlayer(player, dto.OwnerSummary.SummonerId == player.SummonerId);
                                    IsOwner = dto.OwnerSummary.SummonerId ==
                                              Client.LoginPacket.AllSummonerData.Summoner.SumId;
                                    StartGameButton.IsEnabled = IsOwner;
                                    AddBotBlueTeam.IsEnabled = IsOwner;
                                    AddBotPurpleTeam.IsEnabled = IsOwner;

                                    PurpleTeamListView.Items.Add(lobbyPlayer);

                                    if (Client.Whitelist.Count > 0)
                                        if (!Client.Whitelist.Contains(player.SummonerName.ToLower()))
                                            await Client.PVPNet.BanUserFromGame(Client.GameID, player.AccountId);
                                }
                                else if (playerTeam is BotParticipant)
                                {
                                    var botParticipant = playerTeam as BotParticipant;
                                    var botPlayer = new BotControl();
                                    botPlayer = RenderBot(botParticipant);
                                    PurpleTeamListView.Items.Add(botPlayer);
                                }
                            }
                        }
                        break;
                    case "PRE_CHAMP_SELECT":
                    case "CHAMP_SELECT":
                        if (!LaunchedTeamSelect)
                        {
                            Client.ChampSelectDTO = dto;
                            Client.LastPageContent = Client.Container.Content;
                            Client.SwitchPage(new ChampSelectPage(this));
                            Client.GameStatus = "championSelect";
                            Client.SetChatHover();
                            LaunchedTeamSelect = true;
                        }
                        break;
                }
            }));
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = participant.Nick + " joined the room." + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            }));
        }

        public void Invite_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new InvitePlayersPage().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void newRoom_OnRoomMessage(object sender, Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (msg.Body == "This room is not anonymous")
                    return;

                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.From.Resource + ": "
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (Client.Filter)
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "").Filter() +
                              Environment.NewLine;
                else
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;

                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": "
            };
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
            var lobbyPlayer = new CustomLobbyPlayer
            {
                PlayerName =
                {
                    Content = player.SummonerName
                }
            };
            var uriSource =
                new Uri(
                    Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", player.ProfileIconId + ".png"),
                    UriKind.RelativeOrAbsolute);
            lobbyPlayer.ProfileImage.Source = new BitmapImage(uriSource);
            if (IsOwner)
                lobbyPlayer.OwnerLabel.Visibility = Visibility.Visible;

            lobbyPlayer.Width = 400;
            lobbyPlayer.Margin = new Thickness(0, 0, 0, 5);
            if ((player.SummonerId == Client.LoginPacket.AllSummonerData.Summoner.SumId) ||
                (player.SummonerId != Client.LoginPacket.AllSummonerData.Summoner.SumId && !this.IsOwner))
                lobbyPlayer.BanButton.Visibility = Visibility.Hidden;

            lobbyPlayer.BanButton.Tag = player;
            lobbyPlayer.BanButton.Click += KickAndBan_Click;

            return lobbyPlayer;
        }

        private BotControl RenderBot(BotParticipant BotPlayer)
        {
            var botPlayer = new BotControl();
            var uriSource = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champion", BotPlayer.SummonerName.Replace(" bot", "").Replace(" ", "").Replace("'", "") + ".png"));
            
            botPlayer.PlayerName.Content = BotPlayer.SummonerName;
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
            if (button == null)
                return;

            var banPlayer = (PlayerParticipant)button.Tag;
            await Client.PVPNet.BanUserFromGame(Client.GameID, banPlayer.AccountId);
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

        private Int32 getRandomChampInt()
        {
            var rnd = new Random();
            Int32 r = rnd.Next(bots.Count);

            return bots[r];
        }
        /*
         * team id blue - 100, purple - 200
         * 
         * 
         * */
        private async void AddBotBlueTeam_Click(object sender, RoutedEventArgs e)
        {
            Int32 champint = getRandomChampInt();
            champions champions = champions.GetChampion(champint);
            var champDTO = new ChampionDTO
            {
                Active = true,
                Banned = false,
                BotEnabled = true,
                ChampionId = champint,
                DisplayName = champions.displayName
            };

            var skinlist = new List<ChampionSkinDTO>();
            foreach (Dictionary<string, object> skins in champions.Skins)
            {
                var skin = new ChampionSkinDTO
                {
                    ChampionId = champint,
                    FreeToPlayReward = false
                };
                Int32 skinInt = Convert.ToInt32(skins["id"]);
                skin.SkinId = skinInt;
                /*var champs = new List<ChampionDTO>(Client.PlayerChampions);
                foreach (ChampionDTO x in champs)
                {
                    foreach (ChampionSkinDTO myskin in x.ChampionSkins)
                    {
                        if (myskin.Owned)
                        {
                        }
                    }
                }*/
                skin.Owned = true;
                skinlist.Add(skin);
            }
            champDTO.ChampionSkins = skinlist;

            var par = new BotParticipant
            {
                BotSkillLevelName = "Beginner",
                BotSkillLevel = 0,
                Champion = champDTO,
                TeamId = 100,
                pickMode = 0,
                IsGameOwner = false,
                SummonerInternalName = "bot_" + champions.name + "_100", //probably?
                PickTurn = 0,
                IsMe = false,
                Badges = 0,
                TeamName = null,
                Team = 0,
                SummonerName = champions.displayName + " bot"
            };

            await Client.PVPNet.SelectBotChampion(champint, par);


            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() => { }));
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