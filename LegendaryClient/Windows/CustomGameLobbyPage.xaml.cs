using jabber.connection;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Maps;
using PVPNetConnect.RiotObjects.Platform.Game;
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
using System.Xml;

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
        private double GameId;
        private int MapId;
        private Room newRoom;

        public CustomGameLobbyPage()
        {
            InitializeComponent();
            Client.IsInGame = true;
            GameName.Content = Client.GameName;
            Client.OnFixLobby += GameLobby_OnMessageReceived;
            Client.PVPNet.OnMessageReceived += GameLobby_OnMessageReceived;
            //If client has created game use initial DTO
            if (Client.GameLobbyDTO != null)
            {
                GameLobby_OnMessageReceived(null, Client.GameLobbyDTO);
            }

            foreach (string s in Client.Whitelist)
            {
                WhitelistListBox.Items.Add(s);
            }

            Client.LastPageContent = this.Content;
            Client.LobbyContent = this.Content;
            Client.InviteListView = InviteListView;
            Client.InviteListView.Items.Clear();
            Client.OnMessage += Client_OnMessage;
            Client.StatusGrid.Visibility = System.Windows.Visibility.Visible;
            Client.PlayButton.Visibility = System.Windows.Visibility.Collapsed;
            Client.GameStatus = "hostingPracticeGame";
            Client.SetChatHover();
        }

        private void WhitelistAddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(WhiteListTextBox.Text))
            {
                if (!Client.Whitelist.Contains(WhiteListTextBox.Text.ToLower()))
                {
                    Client.Whitelist.Add(WhiteListTextBox.Text.ToLower());
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        WhitelistListBox.Items.Add(WhiteListTextBox.Text);
                        WhiteListTextBox.Text = "";
                    }));
                }
            }
        }

        private void WhitelistListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WhitelistListBox.SelectedIndex != -1)
            {
                WhitelistRemoveButton.IsEnabled = true;
            }
        }

        private void WhitelistRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (WhitelistListBox.SelectedIndex != -1)
            {
                if (Client.Whitelist.Count == 1)
                    WhitelistRemoveButton.IsEnabled = false;
                Client.Whitelist.Remove(WhitelistListBox.SelectedValue.ToString().ToLower());
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    WhitelistListBox.Items.Remove(WhitelistListBox.SelectedValue);
                }));
            }
        }

        public async void Client_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            if (msg.Subject != null)
            {
                ChatSubjects subject = (ChatSubjects)Enum.Parse(typeof(ChatSubjects), msg.Subject, true);
                double[] Double = new double[1] { Convert.ToDouble(msg.From.User.Replace("sum", "")) };
                string[] Name = await Client.PVPNet.GetSummonerNames(Double);
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    InvitePlayer invitePlayer = null;
                    foreach (var x in Client.InviteListView.Items)
                    {
                        InvitePlayer tempInvPlayer = (InvitePlayer)x;
                        if ((string)tempInvPlayer.PlayerLabel.Content == Name[0])
                        {
                            invitePlayer = x as InvitePlayer;
                            break;
                        }
                    }

                    if (subject == ChatSubjects.PRACTICE_GAME_INVITE_ACCEPT)
                    {
                        invitePlayer.StatusLabel.Content = "Accepted";
                        if (IsOwner)
                            Client.Message(msg.From.User, msg.Body, ChatSubjects.PRACTICE_GAME_INVITE_ACCEPT_ACK);
                    }

                    if (subject == ChatSubjects.GAME_INVITE_REJECT)
                    {
                        invitePlayer.StatusLabel.Content = "Rejected";
                    }

                    if (subject == ChatSubjects.GAME_INVITE_LIST_STATUS)
                    {
                        ParseCurrentInvitees(msg.Body);
                    }
                }));
            }
        }

        private void GameLobby_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                GameDTO dto = message as GameDTO;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    MapId = dto.MapId;
                    GameId = dto.Id;
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
                        string ObfuscatedName = Client.GetObfuscatedChatroomName(dto.Name.ToLower() + Convert.ToInt32(dto.Id), ChatPrefixes.Arranging_Practice);
                        string JID = Client.GetChatroomJID(ObfuscatedName, dto.RoomPassword, false);
                        newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
                        newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
                        newRoom.OnRoomMessage += newRoom_OnRoomMessage;
                        newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
                        newRoom.Join(dto.RoomPassword);
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
                            if (playerTeam is PlayerParticipant)
                            {
                                PlayerParticipant player = playerTeam as PlayerParticipant;
                                lobbyPlayer = RenderPlayer(player, dto.OwnerSummary.SummonerId == player.SummonerId);
                                IsOwner = dto.OwnerSummary.SummonerId == Client.LoginPacket.AllSummonerData.Summoner.SumId;
                                StartGameButton.IsEnabled = IsOwner;
                                WhitelistAddButton.IsEnabled = IsOwner;

                                if (Client.Whitelist.Count > 0)
                                {
                                    if (!Client.Whitelist.Contains(player.SummonerName.ToLower()) && player.SummonerId != Client.LoginPacket.AllSummonerData.Summoner.SumId && IsOwner)
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
                    }
                    else if (dto.GameState == "CHAMP_SELECT" || dto.GameState == "PRE_CHAMP_SELECT")
                    {
                        if (!LaunchedTeamSelect)
                        {
                            Client.ChampSelectDTO = dto;
                            Client.LastPageContent = Client.Container.Content;
                            Client.SwitchPage(new ChampSelectPage());
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

        private void newRoom_OnRoomMessage(object sender, jabber.protocol.client.Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (msg.InnerText.Contains("invitelist"))
                {
                    ParseCurrentInvitees(msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", ""));
                    return;
                }

                if (msg.Body != "This room is not anonymous")
                {
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.From.Resource + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
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
            tr.Text = ChatTextBox.Text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            newRoom.PublicMessage(ChatTextBox.Text);
            ChatTextBox.Text = "";
        }

        private CustomLobbyPlayer RenderPlayer(PlayerParticipant player, bool IsOwner)
        {
            CustomLobbyPlayer lobbyPlayer = new CustomLobbyPlayer();
            lobbyPlayer.PlayerName.Content = player.SummonerName;

            string uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", player.ProfileIconId + ".png");
            lobbyPlayer.ProfileImage.Source = Client.GetImage(uriSource);

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

        private void QuitGameButton_Click(object sender, RoutedEventArgs e)
        {
            Client.QuitCurrentGame();
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

        private void InviteButton_Click(object sender, RoutedEventArgs e)
        {
            Client.OverlayContainer.Content = new InvitePlayersPage(GameId, MapId).Content;
            Client.OverlayContainer.Visibility = System.Windows.Visibility.Visible;
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

        private void ParseCurrentInvitees(string Message)
        {
            Client.InviteListView.Items.Clear();
            using (XmlReader reader = XmlReader.Create(new StringReader(Message)))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "invitee":
                                InvitePlayer invitePlayer = new InvitePlayer();
                                invitePlayer.StatusLabel.Content = Client.TitleCaseString(reader.GetAttribute("status"));
                                invitePlayer.PlayerLabel.Content = reader.GetAttribute("name");
                                Client.InviteListView.Items.Add(invitePlayer);
                                break;
                        }
                    }
                }
            }
        }
    }

    public class PlayerItem
    {
        public string Username { get; set; }

        public PlayerParticipant Participant { get; set; }
    }
}