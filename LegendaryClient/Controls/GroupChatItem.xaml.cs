#region

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using jabber;
using jabber.connection;
using jabber.protocol.client;
using LegendaryClient.Logic;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for GroupChatItem.xaml
    /// </summary>
    public partial class GroupChatItem
    {
        private readonly Room _newRoom;

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
                _newRoom = Client.ConfManager.GetRoom(new JID(ChatId));
            }
            catch
            {
                return;
            }

            _newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            _newRoom.OnRoomMessage += GroupChatClient_OnMessage;
            _newRoom.OnParticipantJoin += GroupChatClient_OnParticipantJoin;
            _newRoom.OnParticipantLeave += GroupChatClient_OnParticipantLeave;
            _newRoom.Join();

            RefreshRoom();
        }

        public string ChatId { get; set; }
        public string GroupTitle { get; set; }

        private async void RefreshRoom()
        {
            ParticipantList.Items.Clear();
            foreach (RoomParticipant par in _newRoom.Participants)
            {
                var player = new GroupChatPlayer {SName = {Content = par.Nick}};
                var summoner = await Client.PVPNet.GetSummonerByName(par.Nick);
                var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon",
                    summoner.ProfileIconId + ".png");
                player.SIcon.Source = Client.GetImage(uriSource);
                ParticipantList.Items.Add(player);
                //add to ParticipantList
            }
            ParticipantList.Items.Refresh();
        }

        private void GroupChatClient_OnParticipantLeave(Room room, RoomParticipant participant)
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

        private async void GroupChatClient_OnParticipantJoin(Room room, RoomParticipant participant)
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
                var summoner = await Client.PVPNet.GetSummonerByName(participant.Nick);
                var uriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon",
                    summoner.ProfileIconId + ".png");
                x.SIcon.Source = Client.GetImage(uriSource);
                ParticipantList.Items.Add(x);
                ParticipantList.Items.Refresh();
            }));
        }

        public void GroupChatClient_OnMessage(object sender, Message msg)
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

                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", string.Empty) + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

                ChatText.ScrollToEnd();
            }));
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            _newRoom.OnRoomMessage -= GroupChatClient_OnMessage;
            _newRoom.OnParticipantJoin -= GroupChatClient_OnParticipantJoin;
            _newRoom.OnParticipantLeave -= GroupChatClient_OnParticipantLeave;
            Client.ClearMainGrid(typeof (GroupChatItem));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _newRoom.OnRoomMessage -= GroupChatClient_OnMessage;
            _newRoom.OnParticipantJoin -= GroupChatClient_OnParticipantJoin;
            _newRoom.OnParticipantLeave -= GroupChatClient_OnParticipantLeave;
            Client.ClearMainGrid(typeof (GroupChatItem));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(ChatTextBox.Text))
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

            _newRoom.PublicMessage(ChatTextBox.Text);
            ChatTextBox.Text = string.Empty;
            ChatText.ScrollToEnd();
        }
    }
}