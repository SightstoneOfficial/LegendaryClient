using jabber.connection;
using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for GroupChatItem.xaml
    /// </summary>
    public partial class GroupChatItem : UserControl
    {
        public string ChatID { get; set; }
        public string GroupTitle { get; set; }
        private Room newRoom;

        public GroupChatItem(string id, string title)
        {
            InitializeComponent();
            ChatID = id;
            PlayerLabelName.Content = title;
            GroupTitle = title;
            if (ChatID == null)
                return;
            try
            {
                newRoom = Client.ConfManager.GetRoom(new jabber.JID(ChatID));
            }
            catch
            {
                return;
            }
            
            newRoom.Nickname = Client.LoginPacket.AllSummonerData.Summoner.Name;
            newRoom.OnRoomMessage += GroupChatClient_OnMessage;
            newRoom.OnParticipantJoin += GroupChatClient_OnParticipantJoin;
            newRoom.OnParticipantLeave += GroupChatClient_OnParticipantLeave;
            newRoom.Join();
            refreshRoom();
        }

        private async void refreshRoom()
        {
            ParticipantList.Items.Clear();
            foreach (RoomParticipant par in newRoom.Participants)
            {
                GroupChatPlayer player = new GroupChatPlayer();
                player.SName.Content = par.Nick;
                PublicSummoner summoner = await Client.PVPNet.GetSummonerByName(par.Nick);
                var uriSource = System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", summoner.ProfileIconId + ".png");
                player.sIcon.Source = Client.GetImage(uriSource);
                ParticipantList.Items.Add(player);
                //add to ParticipantList
            }
            ParticipantList.Items.Refresh();
        }

        private void GroupChatClient_OnParticipantLeave(Room room, RoomParticipant participant)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " left the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatText.ScrollToEnd();
                foreach (GroupChatPlayer x in ParticipantList.Items)
                {
                    if (participant.Nick == (string)x.SName.Content)
                    {
                        ParticipantList.Items.Remove(x);
                        ParticipantList.Items.Refresh();
                        break;
                    }
                }
            }));
        }

        private async void GroupChatClient_OnParticipantJoin(Room room, RoomParticipant participant)
        {
            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async() =>
            {
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = participant.Nick + " joined the room." + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                ChatText.ScrollToEnd();
                var x = new GroupChatPlayer();
                x.SName.Content = participant.Nick;
                PublicSummoner summoner = await Client.PVPNet.GetSummonerByName(participant.Nick);
                var uriSource = System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", summoner.ProfileIconId + ".png");
                x.sIcon.Source = Client.GetImage(uriSource);
                ParticipantList.Items.Add(x);
                ParticipantList.Items.Refresh();
            }));
        }

        public void GroupChatClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {

                if (msg.Body != "This room is not anonymous")
                {
                    TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.From.Resource + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Blue);
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    tr.Text = msg.InnerText.Replace("<![CDATA[", "").Replace("]]>", "") + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                    ChatText.ScrollToEnd();
                }
            }));
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            newRoom.OnRoomMessage -= GroupChatClient_OnMessage;
            newRoom.OnParticipantJoin -= GroupChatClient_OnParticipantJoin;
            newRoom.OnParticipantLeave -= GroupChatClient_OnParticipantLeave;
            Client.ClearMainGrid(typeof(GroupChatItem));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            newRoom.OnRoomMessage -= GroupChatClient_OnMessage;
            newRoom.OnParticipantJoin -= GroupChatClient_OnParticipantJoin;
            newRoom.OnParticipantLeave -= GroupChatClient_OnParticipantLeave;
            Client.ClearMainGrid(typeof(GroupChatItem));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            tr.Text = ChatTextBox.Text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            if (String.IsNullOrEmpty(ChatTextBox.Text))
                return;
            newRoom.PublicMessage(ChatTextBox.Text);
            ChatTextBox.Text = "";
            ChatText.ScrollToEnd();
        }
    }
}