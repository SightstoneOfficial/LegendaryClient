﻿using Sightstone.Controls;
using Sightstone.Logic;
using Sightstone.Logic.Maps;
using Sightstone.Properties;
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
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using RtmpSharp.Messaging;
using agsXMPP;
using agsXMPP.protocol.client;
using agsXMPP.protocol.x.muc;
using agsXMPP.Collections;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for FactionsGameLobbyPage.xaml
    /// </summary>
    public partial class FactionsGameLobbyPage
    {
        private string leftTeam;
        private string rightTeam;
        private bool HasConnectedToChat;
        private bool IsOwner;
        private bool LaunchedTeamSelect;
        private double OptomisticLock;
        private MucManager newRoom;
        private Jid jid;
        private static UserClient userClient;

        public FactionsGameLobbyPage()
        {
            InitializeComponent();
            userClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            GameName.Content = userClient.GameName;
            userClient.RiotConnection.MessageReceived += GameLobby_OnMessageReceived;
            //If client has created game use initial DTO
            if (userClient.GameLobbyDTO != null)
            {
                Lobby_OnMessageReceived(null, userClient.GameLobbyDTO);
            }
            Client.InviteListView = InviteListView;
            if (userClient.GameLobbyDTO != null)
            {
                string result =
                    GameName.Content.ToString()
                        .Replace("FACTIONS – ", "")
                        .Replace(" vs. ", "|")
                        .Replace(userClient.GameLobbyDTO.OwnerSummary.SummonerName + "'s game– ", "");
                string[] x = result.Split('|');
                if (x.Length == 2)
                {
                    leftTeam = x[0];
                    rightTeam = x[1];
                    LeftTeamLabel.Content = leftTeam;
                    RightTeamLabel.Content = rightTeam;
                }
            }
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Factions Game Lobby";
            Client.CurrentPage = this;
        }

        private void GameLobby_OnMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            Lobby_OnMessageReceived(sender, message.Body);
        }

        private void Lobby_OnMessageReceived(object sender, object message)
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
                        userClient.LoginPacket.GameTypeConfigs.Find(x => x.Id == dto.GameTypeConfigId);
                    TypeLabel.Content = GetGameMode(configType.Id);
                    SizeLabel.Content = dto.MaxNumPlayers / 2 + "v" + dto.MaxNumPlayers / 2;

                    HasConnectedToChat = true;
                    string obfuscatedName =
                        Client.GetObfuscatedChatroomName(dto.Name.ToLower() + Convert.ToInt64(dto.Id),
                            ChatPrefixes.Arranging_Practice);
                    string Jid = Client.GetChatroomJid(obfuscatedName, dto.RoomPassword, false);
                    newRoom = new MucManager(userClient.XmppConnection);
                    userClient.XmppConnection.OnMessage += XmppConnection_OnMessage;
                    userClient.XmppConnection.OnPresence += XmppConnection_OnPresence;
                    jid = new Jid(dto.RoomName);
                    newRoom.AcceptDefaultConfiguration(jid);
                    newRoom.JoinRoom(jid, userClient.LoginPacket.AllSummonerData.Summoner.Name, dto.RoomPassword);
                }
                switch (dto.GameState)
                {
                    case "TEAM_SELECT":
                        {
                            OptomisticLock = dto.OptimisticLock;
                            LaunchedTeamSelect = false;
                            BlueTeamListView.Items.Clear();
                            PurpleTeamListView.Items.Clear();

                            var allParticipants = new List<Participant>(dto.TeamOne.ToArray());
                            allParticipants.AddRange(dto.TeamTwo);

                            int i = 0;
                            bool purpleSide = false;

                            foreach (Participant playerTeam in allParticipants)
                            {
                                i++;
                                var lobbyPlayer = new CustomLobbyPlayer();
                                //var botPlayer = new BotControl();
                                if (playerTeam is PlayerParticipant)
                                {
                                    var player = playerTeam as PlayerParticipant;
                                    lobbyPlayer = RenderPlayer(player, dto.OwnerSummary.SummonerId == player.SummonerId);
                                    ///BotParticipant botParticipant = playerTeam as BotParticipant;
                                    //botPlayer = RenderBot(botParticipant);
                                    IsOwner = dto.OwnerSummary.SummonerId ==
                                              userClient.LoginPacket.AllSummonerData.Summoner.SumId;
                                    StartGameButton.IsEnabled = IsOwner;

                                    if (userClient.Whitelist.Count > 0)
                                        if (!userClient.Whitelist.Contains(player.SummonerName.ToLower()))
                                            await userClient.calls.BanUserFromGame(userClient.GameID, player.AccountId);
                                }

                                if (i > dto.TeamOne.Count)
                                {
                                    i = 0;
                                    purpleSide = true;
                                }

                                if (!purpleSide)
                                    BlueTeamListView.Items.Add(lobbyPlayer);
                                else
                                    PurpleTeamListView.Items.Add(lobbyPlayer);
                            }
                        }
                        break;
                    case "PRE_CHAMP_SELECT":
                    case "CHAMP_SELECT":
                        if (!LaunchedTeamSelect)
                        {
                            userClient.ChampSelectDTO = dto;
                            Client.LastPageContent = Client.Container.Content;
                            Client.SwitchPage(new ChampSelectPage(dto.RoomName, dto.RoomPassword).Load(this));
                            LaunchedTeamSelect = true;
                        }
                        break;
                }
            }));
        }

        void XmppConnection_OnPresence(object sender, Presence pres)
        {
            if (jid.Bare.Contains(pres.From.User))
                return;

            //It doesn't matter if they leave
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
            if (jid.Bare.Contains(msg.From.User))
                return;

            if (msg.From.Resource == userClient.LoginPacket.AllSummonerData.Summoner.Name)
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
                if (userClient.Filter)
                    tr.Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", "").Filter() +
                              Environment.NewLine;
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
                Text = userClient.LoginPacket.AllSummonerData.Summoner.Name + ": "
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            if (userClient.Filter)
                tr.Text = ChatTextBox.Text.Filter() + Environment.NewLine;
            else
                tr.Text = ChatTextBox.Text + Environment.NewLine;

            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            userClient.XmppConnection.Send(new Message(jid, MessageType.groupchat, ChatTextBox.Text));
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
            var UriSource =
                new System.Uri(
                    Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", player.ProfileIconId + ".png"),
                    UriKind.RelativeOrAbsolute);
            lobbyPlayer.ProfileImage.Source = new BitmapImage(UriSource);
            if (IsOwner)
                lobbyPlayer.OwnerLabel.Visibility = Visibility.Visible;

            lobbyPlayer.Width = 400;
            lobbyPlayer.Margin = new Thickness(0, 0, 0, 5);
            if ((player.SummonerId == userClient.LoginPacket.AllSummonerData.Summoner.SumId) ||
                (player.SummonerId != userClient.LoginPacket.AllSummonerData.Summoner.SumId && !this.IsOwner))
                lobbyPlayer.BanButton.Visibility = Visibility.Hidden;

            lobbyPlayer.BanButton.Tag = player;
            lobbyPlayer.BanButton.Click += KickAndBan_Click;

            return lobbyPlayer;
        }

        private async void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            await userClient.calls.QuitGame();
            Client.ReturnButton.Visibility = Visibility.Hidden;
            Client.SwitchPage(Client.MainPage);
            Client.ClearPage(typeof(FactionsGameLobbyPage)); //Clear pages
            Client.ClearPage(typeof(FactionsCreateGamePage));
        }

        private async void SwitchTeamsButton_Click(object sender, RoutedEventArgs e)
        {
            await userClient.calls.SwitchTeams(userClient.GameID);
        }

        private async void KickAndBan_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var banPlayer = (PlayerParticipant)button.Tag;
            await userClient.calls.BanUserFromGame(userClient.GameID, banPlayer.AccountId);
        }

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            await userClient.calls.StartChampionSelection(userClient.GameID, OptomisticLock);
        }

        public static string GetGameMode(int i)
        {
            var result = userClient.LoginPacket.GameTypeConfigs.Find(x => x.Id == i).Name;
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
                    return result;
            }
        }

        public string GetLeftTeam()
        {
            return leftTeam;
        }

        public string GetRightTeam()
        {
            return rightTeam;
        }
    }
}
