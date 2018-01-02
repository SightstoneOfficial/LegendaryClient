﻿using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.Maps;
using Sightstone.Logic.SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Sightstone.Logic.JSON;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using RtmpSharp.Messaging;
using agsXMPP.protocol.x.muc;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.Collections;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for CustomGameLobbyPage.xaml
    /// </summary>
    public partial class CustomGameLobbyPage
    {
        private static readonly List<int> bots = new List<int>();

        private bool HasConnectedToChat;

        private bool LaunchedTeamSelect;
        private double OptomisticLock;
        private MucManager newRoom;
        private static Jid roomJid;
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];

        public CustomGameLobbyPage(GameDTO gameLobby = null)
        {
            InitializeComponent();
            //Hopefully this works better
            foreach (var champs in UserClient.PlayerChampions.Where(champs => champs.BotEnabled))
                bots.Add(champs.ChampionId);
            GameName.Content = UserClient.GameName;
            UserClient.RiotConnection.MessageReceived += GameLobby_OnMessageReceived;
            //If client has created game use initial DTO
            if (UserClient.GameLobbyDTO != null)
                Lobby_OnMessageReceived(null, UserClient.GameLobbyDTO);
            else
            {
                UserClient.GameLobbyDTO = gameLobby;
                Lobby_OnMessageReceived(null, UserClient.GameLobbyDTO);
            }
            Client.InviteListView = InviteListView;
            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Custom Game Lobby";
        }

        private void GameLobby_OnMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            Lobby_OnMessageReceived(sender, message.Body);
        }

        private void Lobby_OnMessageReceived(object sender, object message)
        {
            if (message == null)
                return;

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
                    GameTypeConfigDTO configType = UserClient.LoginPacket.GameTypeConfigs.Find(x => x.Id == dto.GameTypeConfigId);
                    TypeLabel.Content = GetGameMode(configType.Id);
                    SizeLabel.Content = dto.MaxNumPlayers / 2 + "v" + dto.MaxNumPlayers / 2;

                    HasConnectedToChat = true;

                    string obfuscatedName = Client.GetObfuscatedChatroomName(dto.Name.ToLower() + Convert.ToInt64(dto.Id), ChatPrefixes.Arranging_Practice);
                    string Jid = Client.GetChatroomJid(obfuscatedName, dto.RoomPassword, false);
                    newRoom = new MucManager(UserClient.XmppConnection);
                    UserClient.XmppConnection.OnMessage +=XmppConnection_OnMessage;
                    UserClient.XmppConnection.OnPresence += XmppConnection_OnPresence;
                    roomJid = new Jid(Jid);
                    newRoom.AcceptDefaultConfiguration(roomJid);
                    newRoom.JoinRoom(roomJid, UserClient.LoginPacket.AllSummonerData.Summoner.Name);
                }
                switch (dto.GameState)
                {
                    case "TEAM_SELECT":
                        {
                            bool isSpectator = false;
                            OptomisticLock = dto.OptimisticLock;
                            LaunchedTeamSelect = false;
                            BlueTeamListView.Items.Clear();
                            PurpleTeamListView.Items.Clear();
                            SpectatorListView.Items.Clear();

                            foreach (Participant playerTeam in dto.TeamOne)
                            {
                                if (playerTeam is PlayerParticipant)
                                {
                                    var lobbyPlayer = new CustomLobbyPlayer();
                                    var player = playerTeam as PlayerParticipant;
                                    lobbyPlayer = RenderPlayer(player, dto.OwnerSummary.SummonerId == player.SummonerId);
                                    UserClient.isOwnerOfGame = dto.OwnerSummary.SummonerId == UserClient.LoginPacket.AllSummonerData.Summoner.SumId;
                                    StartGameButton.IsEnabled = UserClient.isOwnerOfGame;
                                    AddBotBlueTeam.IsEnabled = UserClient.isOwnerOfGame;
                                    AddBotPurpleTeam.IsEnabled = UserClient.isOwnerOfGame;

                                    BlueTeamListView.Items.Add(lobbyPlayer);

                                    if (UserClient.Whitelist.Count <= 0)
                                        continue;

                                    if (!UserClient.Whitelist.Contains(player.SummonerName.ToLower()))
                                        await UserClient.calls.BanUserFromGame(UserClient.GameID, player.AccountId);
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
                                    UserClient.isOwnerOfGame = dto.OwnerSummary.SummonerId == UserClient.LoginPacket.AllSummonerData.Summoner.SumId;
                                    StartGameButton.IsEnabled = UserClient.isOwnerOfGame;
                                    AddBotBlueTeam.IsEnabled = UserClient.isOwnerOfGame;
                                    AddBotPurpleTeam.IsEnabled = UserClient.isOwnerOfGame;

                                    PurpleTeamListView.Items.Add(lobbyPlayer);

                                    if (UserClient.Whitelist.Count <= 0)
                                        continue;

                                    if (!UserClient.Whitelist.Contains(player.SummonerName.ToLower()))
                                        await UserClient.calls.BanUserFromGame(UserClient.GameID, player.AccountId);
                                }
                                else if (playerTeam is BotParticipant)
                                {
                                    var botParticipant = playerTeam as BotParticipant;
                                    var botPlayer = new BotControl();
                                    botPlayer = RenderBot(botParticipant);
                                    PurpleTeamListView.Items.Add(botPlayer);
                                }
                            }
                            foreach (GameObserver observer in dto.Observers)
                            {
                                if (observer.SummonerId == UserClient.LoginPacket.AllSummonerData.Summoner.SumId)
                                    isSpectator = true;

                                var spectatorItem = new CustomLobbyObserver();

                                spectatorItem = RenderObserver(observer);
                                SpectatorListView.Items.Add(spectatorItem);
                            }
                            if (isSpectator)
                            {
                                AddBotPurpleTeam.Visibility = Visibility.Hidden;
                                AddBotBlueTeam.Visibility = Visibility.Hidden;
                                JoinBlueTeamFromSpectator.Visibility = Visibility.Visible;
                                JoinPurpleTeamFromSpectator.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                AddBotPurpleTeam.Visibility = Visibility.Visible;
                                AddBotBlueTeam.Visibility = Visibility.Visible;
                                JoinBlueTeamFromSpectator.Visibility = Visibility.Hidden;
                                JoinPurpleTeamFromSpectator.Visibility = Visibility.Hidden;
                            }
                        }
                        break;
                    case "PRE_CHAMP_SELECT":
                    case "CHAMP_SELECT":
                        if (!LaunchedTeamSelect)
                        {
                            UserClient.ChampSelectDTO = dto;
                            Client.LastPageContent = Client.Container.Content;
                            Client.SwitchPage(new ChampSelectPage(dto.RoomName, dto.RoomPassword).Load(this));
                            UserClient.GameStatus = "championSelect";
                            UserClient.SetChatHover();
                            LaunchedTeamSelect = true;
                        }
                        break;
                }
            }));
        }

        void XmppConnection_OnPresence(object sender, Presence pres)
        {
            if (pres.To.Bare != roomJid.Bare)
                return;
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = pres.From.Resource + " joined the room." + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            }));
        }

        void XmppConnection_OnMessage(object sender, Message msg)
        {
            if (msg.From.Resource == UserClient.LoginPacket.AllSummonerData.Summoner.Name)
                return;

            if (msg.From.Resource == UserClient.LoginPacket.AllSummonerData.Summoner.Name)
                return;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (msg.Body == "This room is not anonymous")
                    return;

                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.From.Resource + ": "
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Turquoise);
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (UserClient.Filter)
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "").Filter() + Environment.NewLine;
                else
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;

                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }));
        }

        public void Invite_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new InvitePlayersPage().Content;
            Client.OverlayContainer.Visibility = Visibility.Visible;
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = UserClient.LoginPacket.AllSummonerData.Summoner.Name + ": "
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            if (UserClient.Filter)
                tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
            else
                tr.Text = ChatTextBox.Text + Environment.NewLine;

            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            UserClient.XmppConnection.Send(new Message(roomJid, MessageType.groupchat, ChatTextBox.Text));
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
            var UriSource = Client.GetIconUri(player.ProfileIconId);
            lobbyPlayer.ProfileImage.Source = new BitmapImage(UriSource);
            if (IsOwner)
                lobbyPlayer.OwnerLabel.Visibility = Visibility.Visible;

            lobbyPlayer.Width = 400;
            lobbyPlayer.Margin = new Thickness(0, 0, 0, 5);
            if ((player.SummonerId == UserClient.LoginPacket.AllSummonerData.Summoner.SumId) || (player.SummonerId != UserClient.LoginPacket.AllSummonerData.Summoner.SumId && !UserClient.isOwnerOfGame))
                lobbyPlayer.BanButton.Visibility = Visibility.Hidden;

            lobbyPlayer.BanButton.Tag = player;
            lobbyPlayer.BanButton.Click += KickAndBan_Click;

            return lobbyPlayer;
        }

        private BotControl RenderBot(BotParticipant BotPlayer)
        {
            var botPlayer = new BotControl();
            champions champ = champions.GetChampion(BotPlayer.SummonerInternalName.Split('_')[1]);
            var Source = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champion", champ.name + "_Square_0.png"));

            botPlayer.Width = 400;
            botPlayer.Margin = new Thickness(0, 0, 0, 5);
            botPlayer.PlayerName.Content = BotPlayer.SummonerName;
            botPlayer.ProfileImage.Source = new BitmapImage(Source);
            botPlayer.blueSide = BotPlayer.SummonerInternalName.Split('_')[2] == "100";
            botPlayer.difficulty = BotPlayer.BotSkillLevel;
            botPlayer.cmbSelectDificulty.Visibility = UserClient.isOwnerOfGame ? Visibility.Visible : Visibility.Hidden;
            botPlayer.cmbSelectDificulty.Items.Add("Beginner");
            botPlayer.cmbSelectDificulty.Items.Add("Intermediate");
            botPlayer.cmbSelectDificulty.Items.Add("Doom");
            botPlayer.cmbSelectDificulty.Items.Add("Intro");
            botPlayer.cmbSelectDificulty.SelectedIndex = BotPlayer.BotSkillLevel;
            champ.Skins.Sort((x, y) => { return x.Num.CompareTo(y.Num); });
            foreach (var skin in champ.Skins)
                botPlayer.cmbSelectSkin.Items.Add(skin.Name);
            foreach (int bot in bots)
                botPlayer.cmbSelectChamp.Items.Add(champions.GetChampion(bot).name);

            botPlayer.cmbSelectSkin.SelectedIndex = BotPlayer.LastSelectedSkinIndex;

            botPlayer.cmbSelectChamp.Visibility = UserClient.isOwnerOfGame ? Visibility.Visible : Visibility.Hidden;
            botPlayer.cmbSelectChamp.SelectedItem = champ.name;
            botPlayer.BanButton.Visibility = UserClient.isOwnerOfGame ? Visibility.Visible : Visibility.Hidden;
            botPlayer.BanButton.Tag = BotPlayer;
            botPlayer.BanButton.Click += KickAndBan_Click;
            botPlayer.cmbSelectChamp.SelectionChanged += async (a, b) =>
            {
                champions c = champions.GetChampion((string)botPlayer.cmbSelectChamp.SelectedValue);
                await UserClient.calls.RemoveBotChampion(champ.id, BotPlayer);
                AddBot(c.id, botPlayer.blueSide, botPlayer.difficulty);
            };
            botPlayer.cmbSelectDificulty.SelectionChanged += async (a, b) =>
            {
                champions c = champions.GetChampion((string)botPlayer.cmbSelectChamp.SelectedValue);
                await UserClient.calls.RemoveBotChampion(champ.id, BotPlayer);
                AddBot(c.id, botPlayer.blueSide, botPlayer.cmbSelectDificulty.SelectedIndex, botPlayer.cmbSelectSkin.SelectedIndex);
            };
            botPlayer.cmbSelectSkin.SelectionChanged += async (a, b) =>
            {
                champions c = champions.GetChampion((string)botPlayer.cmbSelectChamp.SelectedValue);
                await UserClient.calls.RemoveBotChampion(champ.id, BotPlayer);
                AddBot(c.id, botPlayer.blueSide, botPlayer.cmbSelectDificulty.SelectedIndex, botPlayer.cmbSelectSkin.SelectedIndex);
            };

            return botPlayer;
        }

        private CustomLobbyObserver RenderObserver(GameObserver observer)
        {
            var lobbyPlayer = new CustomLobbyObserver
            {
                PlayerName =
                {
                    Content = observer.SummonerName
                }
            };
            var UriSource = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", observer.ProfileIconId + ".png"), UriKind.RelativeOrAbsolute);
            lobbyPlayer.ProfileImage.Source = new BitmapImage(UriSource);

            lobbyPlayer.Width = 250;
            lobbyPlayer.Margin = new Thickness(0, 0, 0, 5);
            if ((observer.SummonerId == UserClient.LoginPacket.AllSummonerData.Summoner.SumId) || (observer.SummonerId != UserClient.LoginPacket.AllSummonerData.Summoner.SumId && !UserClient.isOwnerOfGame))
                lobbyPlayer.BanButton.Visibility = Visibility.Hidden;

            lobbyPlayer.BanButton.Tag = observer;
            lobbyPlayer.BanButton.Click += KickAndBanObserver_Click;

            return lobbyPlayer;
        }

        private async void KickAndBanObserver_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var player = button.Tag as GameObserver;
            await UserClient.calls.BanObserverFromGame(UserClient.GameID, player.AccountId);
        }

        private async void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            await UserClient.calls.QuitGame();
            Client.ReturnButton.Visibility = Visibility.Hidden;
            Client.SwitchPage(Client.MainPage);
            Client.ClearPage(typeof(CustomGameLobbyPage)); //Clear pages
            Client.ClearPage(typeof(CreateCustomGamePage));
        }

        private async void SwitchTeamsButton_Click(object sender, RoutedEventArgs e)
        {
            await UserClient.calls.SwitchTeams(UserClient.GameID);
        }

        private static async void KickAndBan_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var player = button.Tag as PlayerParticipant;
            if (player != null)
            {
                PlayerParticipant banPlayer = player;
                await UserClient.calls.BanUserFromGame(UserClient.GameID, banPlayer.AccountId);
            }
            else
            {
                var tag = button.Tag as BotParticipant;
                if (tag == null)
                    return;

                BotParticipant banPlayer = tag;
                await UserClient.calls.RemoveBotChampion(champions.GetChampion(banPlayer.SummonerInternalName.Split('_')[1]).id, banPlayer);
            }
        }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            await UserClient.calls.StartChampionSelection(UserClient.GameID, OptomisticLock);
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
                    return UserClient.LoginPacket.GameTypeConfigs.Find(x => x.Id == i).Name;
            }
        }

        private static int GetRandomChampInt()
        {
            var rnd = new Random();
            int r = rnd.Next(bots.Count);

            return bots[r];
        }

        private static async void AddBot(int id, bool blueSide, int difficulty, int skinIndex = 0)
        {
            int champint = (id == 0 ? GetRandomChampInt() : id);
            champions champions = champions.GetChampion(champint);
            var champDTO = new ChampionDTO
            {
                Active = true,
                Banned = false,
                BotEnabled = true,
                ChampionId = champint,
                DisplayName = champions.displayName
            };

            List<ChampionSkinDTO> skinlist = new List<ChampionSkinDTO>();
            foreach(var skins in champions.Skins)
            {
                skinlist.Add(new ChampionSkinDTO {
                                                  ChampionId = champint,
                                                  SkinId = skins.Id,
                                                  SkinIndex = skins.Num,
                                                  StillObtainable = true
                                              });
            }

            champDTO.ChampionSkins = skinlist;

            var par = new BotParticipant
            {
                Champion = champDTO,
                pickMode = 0,
                IsGameOwner = false,
                PickTurn = 0,
                IsMe = false,
                Badges = 0,
                TeamName = null,
                Team = 0,
                SummonerName = champions.displayName + " bot",
                LastSelectedSkinIndex = skinIndex
            };
            if (blueSide)
            {
                par.teamId = "100";
                par.SummonerInternalName = "bot_" + champions.name + "_100";
            }
            else
            {
                par.teamId = "200";
                par.SummonerInternalName = "bot_" + champions.name + "_200";
            }
            switch (difficulty)
            {
                case 0:
                    par.botSkillLevelName = "Beginner";
                    par.BotSkillLevel = difficulty;
                    break;
                case 1:
                    par.botSkillLevelName = "Intermediate";
                    par.BotSkillLevel = difficulty;
                    break;
                case 2:
                    par.botSkillLevelName = "Doom";
                    par.BotSkillLevel = difficulty;
                    break;
                case 3:
                    par.botSkillLevelName = "Intro";
                    par.BotSkillLevel = difficulty;
                    break;
            }
            await UserClient.calls.SelectBotChampion(champint, par);
        }

        private void AddBotBlueTeam_Click(object sender, RoutedEventArgs e)
        {
            AddBot(0, true, 0);
        }

        private void AddBotPurpleTeam_Click(object sender, RoutedEventArgs e)
        {
            AddBot(0, false, 0);
        }

        private async void SpectatorButton_Click(object sender, RoutedEventArgs e)
        {
            await UserClient.calls.SwitchPlayerToObserver(UserClient.GameID);
        }

        private async void JoinPurpleTeamFromSpectator_Click(object sender, RoutedEventArgs e)
        {
            await UserClient.calls.SwitchObserverToPlayer(UserClient.GameID, 200);
        }

        private async void JoinBlueTeamFromSpectator_Click(object sender, RoutedEventArgs e)
        {
            await UserClient.calls.SwitchObserverToPlayer(UserClient.GameID, 100);
        }
    }
}
