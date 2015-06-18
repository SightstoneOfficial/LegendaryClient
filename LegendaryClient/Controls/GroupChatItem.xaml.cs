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
using agsXMPP.Collections;

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
            Client.XmppConnection.MessageGrabber.Add(new Jid(ChatId), new BareJidComparer(), new MessageCB(XmppConnection_OnMessage), null);
            newRoom.AcceptDefaultConfiguration(new Jid(ChatId));
            roomName = ChatId;
            newRoom.JoinRoom(new Jid(ChatId), Client.LoginPacket.AllSummonerData.Summoner.Name);

            RefreshRoom();
        }

        void XmppConnection_OnMessage(object sender, Message msg, object data)
        {
            if (msg.To.Bare != roomName)
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

                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", string.Empty) + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

                ChatText.ScrollToEnd();
            }));
        }

        async void XmppConnection_OnPresence(object sender, Presence pres)
        {
            if (pres.To.Bare != roomName)
                return;
                        
            if (pres.Type != PresenceType.available)
            {
                await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                    {
                        Text = pres.From.User + " left the room." + Environment.NewLine
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);

                    ChatText.ScrollToEnd();
                    foreach (
                        var x in
                            from GroupChatPlayer x in ParticipantList.Items
                            where pres.From.User == (string)x.SName.Content
                            select x)
                    {
                        ParticipantList.Items.Remove(x);
                        ParticipantList.Items.Refresh();
                        break;
                    }
                }));
            }
            else
            {
                await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
                {
                    var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                    {
                        Text = pres.From.User + " joined the room." + Environment.NewLine
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);

                    ChatText.ScrollToEnd();
                    var x = new GroupChatPlayer { SName = { Content = pres.From.User } };
                    var summoner = await RiotCalls.GetSummonerByName(pres.From.User);
                    var UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon",
                        summoner.ProfileIconId + ".png");
                    x.SIcon.Source = Client.GetImage(UriSource);
                    ParticipantList.Items.Add(x);
                    ParticipantList.Items.Refresh();
                }));
            }
            
        }

        public string ChatId { get; set; }
        public string GroupTitle { get; set; }

        //TO FIX
        private void RefreshRoom()
        {

            /*
            ParticipantList.Items.Clear();
            foreach (RoomParticipant par in newRoom.RequestMemberList(new Jid(roomName))
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
            //*/
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Client.XmppConnection.MessageGrabber.Remove(new Jid(ChatId));
            Client.XmppConnection.OnPresence -= XmppConnection_OnPresence;
            newRoom.JoinRoom(new Jid(ChatId), Client.LoginPacket.AllSummonerData.Summoner.Name);
            Client.ClearMainGrid(typeof (GroupChatItem));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.XmppConnection.MessageGrabber.Remove(new Jid(ChatId));
            Client.XmppConnection.OnPresence -= XmppConnection_OnPresence;
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

            Client.XmppConnection.Send(new Message(new Jid(roomName), MessageType.chat, ChatTextBox.Text));
            ChatTextBox.Text = string.Empty;
            ChatText.ScrollToEnd();
        }
    }
}