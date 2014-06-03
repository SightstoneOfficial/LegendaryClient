using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using jabber.protocol.client;
using LegendaryClient.Logic;
using System.Windows.Threading;
using System.Threading;
using LegendaryClient.Controls;
using System.Xml;
using System.IO;
using jabber.connection;
using PVPNetConnect.RiotObjects.Platform.Matchmaking;
using System.Collections;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for TeamQueuePage.xaml
    /// </summary>
    public partial class TeamQueuePage : Page
    {
        Message MessageData;
        long InviteId = 0;
        private Room newRoom;
        bool IsOwner = false;

        /// <summary>
        /// When invited to a team
        /// </summary>
        /// <param name="Message"></param>
        public TeamQueuePage(bool IsCreator)
        {
            InitializeComponent();
            Client.InviteListView = InviteListView;


            Client.PVPNet.Accept(InviteId.ToString());

            string ObfuscatedName = Client.GetObfuscatedChatroomName(InviteId.ToString(), ChatPrefixes.Arranging_Game);
            string JID = Client.GetChatroomJID(ObfuscatedName, "0", true);
            newRoom = Client.ConfManager.GetRoom(new jabber.JID(JID));
            newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            newRoom.OnRoomMessage += newRoom_OnRoomMessage;
            newRoom.OnParticipantJoin += newRoom_OnParticipantJoin;
            newRoom.Join();

            IsOwner = IsCreator;

            if (IsCreator)
            {
                InviteButton.IsEnabled = true;
                StartGameButton.IsEnabled = true;
            }
            else
            {
                InviteButton.IsEnabled = false;
                StartGameButton.IsEnabled = false;
            }

            Client.OnMessage += Client_OnMessage;
        }

        private void newRoom_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                TeamListView.Items.Add(participant.NickJID);
            }));
        }

        private void newRoom_OnRoomMessage(object sender, Message msg)
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

        private async void Client_OnMessage(object sender, Message msg)
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

                    if (subject == ChatSubjects.GAME_INVITE_ACCEPT)
                    {
                        if (invitePlayer != null)
                            invitePlayer.StatusLabel.Content = "Accepted";
                        if (IsOwner)
                            Client.Message("sum222908", "<body><inviteId>8649134</inviteId><userName>Snowl</userName><profileIconId>576</profileIconId><gameType>NORMAL_GAME</gameType><groupId></groupId><seasonRewards>-1</seasonRewards><mapId>1</mapId><queueId>2</queueId><gameMode>classic_pvp</gameMode><gameDifficulty></gameDifficulty></body>", ChatSubjects.GAME_INVITE_ACCEPT_ACK);
                    }
                    else if (subject == ChatSubjects.GAME_INVITE_REJECT)
                    {
                        if (invitePlayer != null)
                            invitePlayer.StatusLabel.Content = "Rejected";
                    }
                    else if (subject == ChatSubjects.GAME_INVITE_LIST_STATUS)
                    {
                        ParseCurrentInvitees(msg.Body);
                    }
                    else if (subject == ChatSubjects.GAME_INVITE_ALLOW_SUGGESTIONS)
                    {
                        InviteButton.IsEnabled = true;
                    }
                    else if (subject == ChatSubjects.GAME_INVITE_DISALLOW_SUGGESTIONS)
                    {
                        InviteButton.IsEnabled = false;
                    }
                    else if (subject == ChatSubjects.GAME_INVITE_OWNER_CANCEL)
                    {
                        MessageOverlay messageOver = new MessageOverlay();
                        messageOver.MessageTitle.Content = "Party Cancelled";
                        messageOver.MessageTextBox.Text = "This party has been cancelled.";
                        newRoom.Leave("Party Cancelled");
                        Client.OverlayContainer.Content = messageOver.Content;
                        Client.OverlayContainer.Visibility = Visibility.Visible;
                    }
                    else if (subject == ChatSubjects.GAME_INVITE_CANCEL)
                    {
                        MessageOverlay messageOver = new MessageOverlay();
                        messageOver.MessageTitle.Content = "Kicked";
                        messageOver.MessageTextBox.Text = "You have been kicked from the party.";
                        newRoom.Leave("Kicked");
                        Client.OverlayContainer.Content = messageOver.Content;
                        Client.OverlayContainer.Visibility = Visibility.Visible;
                    }
                    else if (subject == ChatSubjects.VERIFY_INVITEE)
                    {
                        Client.Message(MessageData.From.User, MessageData.Body, ChatSubjects.VERIFY_INVITEE_ACK);
                    }
                }));
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

        private async void StartGameButton_Click(object sender, RoutedEventArgs e)
        {
            MatchMakerParams pareams = new MatchMakerParams();
            pareams.InvitationId = 8649134;
            pareams.QueueIds = new int[1] { 2 };
            pareams.Team = new List<int>();
            pareams.Team.Add(222908);
            pareams.Team.Add(499467);
            SearchingForMatchNotification par = await Client.PVPNet.AttachTeamToQueue(pareams);

            Client.Message("sum222908", "<body><inviteId>8649134</inviteId><userName>Snooowl</userName><profileIconId>576</profileIconId><gameType>NORMAL_GAME</gameType><groupId></groupId><seasonRewards>-1</seasonRewards><mapId>1</mapId><queueId>2</queueId><gameMode>classic_pvp</gameMode><gameDifficulty></gameDifficulty></body>", ChatSubjects.VERIFY_INVITEE);
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

    }
}