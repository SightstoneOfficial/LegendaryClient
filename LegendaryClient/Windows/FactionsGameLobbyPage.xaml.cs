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
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Game;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for FactionsGameLobbyPage.xaml
    /// </summary>
    public partial class FactionsGameLobbyPage
    {
        private readonly string leftTeam;
        private readonly string rightTeam;
        private bool HasConnectedToChat;
        private bool IsOwner;
        private bool LaunchedTeamSelect;
        private double OptomisticLock;
        private Room newRoom;

        public FactionsGameLobbyPage()
        {
            InitializeComponent();
            Change();

            GameName.Content = Client.GameName;
            Client.PVPNet.OnMessageReceived += GameLobby_OnMessageReceived;
            //If client has created game use initial DTO
            if (Client.GameLobbyDTO != null)
            {
                GameLobby_OnMessageReceived(null, Client.GameLobbyDTO);
            }
            Client.InviteListView = InviteListView;
            if (Client.GameLobbyDTO != null)
            {
                string result =
                    GameName.Content.ToString()
                        .Replace("FACTIONS – ", "")
                        .Replace(" vs. ", "|")
                        .Replace(Client.GameLobbyDTO.OwnerSummary.SummonerName + "'s game– ", "");
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
            if (message.GetType() != typeof (GameDTO))
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
                    SizeLabel.Content = dto.MaxNumPlayers/2 + "v" + dto.MaxNumPlayers/2;

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
                                          Client.LoginPacket.AllSummonerData.Summoner.SumId;
                                StartGameButton.IsEnabled = IsOwner;

                                if (Client.Whitelist.Count > 0)
                                    if (!Client.Whitelist.Contains(player.SummonerName.ToLower()))
                                        await Client.PVPNet.BanUserFromGame(Client.GameID, player.AccountId);
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
                            Client.ChampSelectDTO = dto;
                            Client.LastPageContent = Client.Container.Content;
                            Client.SwitchPage(new ChampSelectPage(dto.RoomName, dto.RoomPassword).Load(this));
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

        private async void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            await Client.PVPNet.QuitGame();
            Client.ReturnButton.Visibility = Visibility.Hidden;
            uiLogic.UpdateMainPage();
            Client.ClearPage(typeof(FactionsGameLobbyPage)); //Clear pages
            Client.ClearPage(typeof(FactionsCreateGamePage)); 
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

            var banPlayer = (PlayerParticipant) button.Tag;
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

        public string GetLeftTeam()
        {
            return leftTeam;
        }

        public string GetRightTeam()
        {
            return rightTeam;
        }
    }

    public class PlayerItem
    {
        public string Username { get; set; }

        public PlayerParticipant Participant { get; set; }
    }
}