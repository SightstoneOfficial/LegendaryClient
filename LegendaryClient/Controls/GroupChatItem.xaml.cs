using LegendaryClient.Logic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Logic.Riot;
using agsXMPP.protocol.client;
using agsXMPP;
using agsXMPP.protocol.x.muc;

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for GroupChatItem.xaml
    /// </summary>
    public partial class GroupChatItem
    {
        private readonly MucManager newRoom;
        private string roomName;

        public GroupChatItem(string id, string title)
        {
            InitializeComponent();
            ChatId = id;
            PlayerLabelName.Content = title;
            GroupTitle = title;
            if (ChatId == null)
                return;
            try
            {
                newRoom = new MucManager(Client.XmppConnection);
            }
            catch
            {
                return;
            }
            Client.XmppConnection.OnPresence += XmppConnection_OnPresence;
            Client.XmppConnection.OnMessage += XmppConnection_OnMessage;
            newRoom.OnParticipantJoin += GroupXmppConnection_OnParticipantJoin;
            newRoom.OnParticipantLeave += GroupXmppConnection_OnParticipantLeave;
            newRoom.AcceptDefaultConfiguration(new Jid(ChatId));
            roomName = ChatId;
            newRoom.JoinRoom(new Jid(ChatId), Client.LoginPacket.AllSummonerData.Summoner.Name);

            RefreshRoom();
        }

        void XmppConnection_OnMessage(object sender, Message msg)
        {
            if(msg.To.Resource.Contains(roomName))
            {
                GroupXmppConnection_OnMessage(sender, msg);
            }
        }

        void XmppConnection_OnPresence(object sender, Presence pres)
        {
            
        }

        public string ChatId { get; set; }
        public string GroupTitle { get; set; }

        private async void RefreshRoom()
        {
            ParticipantList.Items.Clear();
            foreach (RoomParticipant par in newRoom.Participants)
            {
                var player = new GroupChatPlayer {SName = {Content = par.Nick}};
                var summoner = await RiotCalls.GetSummonerByName(par.Nick);
                var UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon",
                    summoner.ProfileIconId + ".png");
                player.SIcon.Source = Client.GetImage(UriSource);
                ParticipantList.Items.Add(player);
                //add to ParticipantList
            }
            ParticipantList.Items.Refresh();
        }

        private void GroupXmppConnection_OnParticipantLeave(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = participant.Nick + " left the room." + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);

                ChatText.ScrollToEnd();
                foreach (
                    var x in
                        from GroupChatPlayer x in ParticipantList.Items
                        where participant.Nick == (string) x.SName.Content
                        select x)
                {
                    ParticipantList.Items.Remove(x);
                    ParticipantList.Items.Refresh();
                    break;
                }
            }));
        }

        private async void GroupXmppConnection_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = participant.Nick + " joined the room." + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);

                ChatText.ScrollToEnd();
                var x = new GroupChatPlayer {SName = {Content = participant.Nick}};
                var summoner = await RiotCalls.GetSummonerByName(participant.Nick);
                var UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon",
                    summoner.ProfileIconId + ".png");
                x.SIcon.Source = Client.GetImage(UriSource);
                ParticipantList.Items.Add(x);
                ParticipantList.Items.Refresh();
            }));
        }

        public void GroupXmppConnection_OnMessage(object sender, Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if (msg.Body == "This room is not anonymous")
                    return;

                var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.From.Resource + ": "
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Turquoise);

                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", string.Empty) + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

                ChatText.ScrollToEnd();
            }));
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Client.XmppConnection.OnMessage -= XmppConnection_OnMessage;
            newRoom.OnParticipantJoin -= GroupXmppConnection_OnParticipantJoin;
            newRoom.OnParticipantLeave -= GroupXmppConnection_OnParticipantLeave;
            newRoom.JoinRoom(new Jid(ChatId), Client.LoginPacket.AllSummonerData.Summoner.Name);
            Client.ClearMainGrid(typeof (GroupChatItem));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.XmppConnection.OnMessage -= XmppConnection_OnMessage;
            newRoom.OnParticipantJoin -= GroupXmppConnection_OnParticipantJoin;
            newRoom.OnParticipantLeave -= GroupXmppConnection_OnParticipantLeave;
            newRoom.LeaveRoom(new Jid(ChatId), Client.LoginPacket.AllSummonerData.Summoner.Name);
            Client.ClearMainGrid(typeof (GroupChatItem));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ChatTextBox.Text))
                return;

            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": "
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);

            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = ChatTextBox.Text + Environment.NewLine
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

            Client.XmppConnection.Send(new Message(new Jid(roomName), ChatTextBox.Text));
            ChatTextBox.Text = string.Empty;
            ChatText.ScrollToEnd();
        }
    }
}