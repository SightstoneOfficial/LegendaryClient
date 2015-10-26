using Sightstone.Logic;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using Sightstone.Logic.Riot;
using agsXMPP.protocol.client;
using agsXMPP;
using agsXMPP.protocol.x.muc;
using agsXMPP.Collections;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.MultiUser;
using System.Windows.Media.Imaging;

namespace Sightstone.Controls
{
    /// <summary>
    ///     Interaction logic for GroupChatItem.xaml
    /// </summary>
    public partial class GroupChatItem
    {
        private readonly MucManager newRoom;
        private string roomName;
        private static UserClient userClient;

        public GroupChatItem(string id, string title)
        {
            InitializeComponent();
            userClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
            MahApps.Metro.Controls.TextBoxHelper.SetWatermark(ChatTextBox, "Sending message from " + userClient.LoginPacket.AllSummonerData.Summoner.InternalName);
            ChatId = id;
            PlayerLabelName.Content = title;
            GroupTitle = title;
            if (ChatId == null)
                return;
            try
            {
                newRoom = new MucManager(userClient.XmppConnection);
            }
            catch
            {
                return;
            }
            userClient.XmppConnection.OnPresence += XmppConnection_OnPresence;
            userClient.XmppConnection.OnMessage += XmppConnection_OnMessage;
            newRoom.AcceptDefaultConfiguration(new Jid(ChatId));
            roomName = ChatId;
            newRoom.JoinRoom(new Jid(ChatId), userClient.LoginPacket.AllSummonerData.Summoner.Name);
        }

        void XmppConnection_OnMessage(object sender, Message msg)
        {
            if (!roomName.Contains(msg.From.User))
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
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("SecondaryAccentBrush"));

                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                {
                    Text = msg.Body.Replace("<![CDATA[", "").Replace("]]>", string.Empty) + Environment.NewLine
                };
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("MaterialDesignBody"));

                ChatText.ScrollToEnd();
            }));
        }

        async void XmppConnection_OnPresence(object sender, Presence pres)
        {
            if (pres.From.Bare != roomName)
                return;
                        
            if (pres.Type != PresenceType.available)
            {
                await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    using (XmlReader reader = XmlReader.Create(new StringReader(pres.InnerXml)))
                    {
                        try
                        {
                            string jid = string.Empty;
                            while (reader.Read())
                            {
                                if (!reader.IsStartElement() || reader.IsEmptyElement)
                                    continue;

                                #region Parse Presence

                                switch (reader.Name)
                                {
                                    case "jid":
                                        reader.Read();
                                        jid = reader.Value;
                                        break;
                                }

                                #endregion Parse Presence
                            }
                            var user = userClient.GetUserFromJid(jid);

                            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                            {
                                Text = user + " left the room." + Environment.NewLine
                            };
                            tr.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("PrimaryHueMidBrush"));
                           
                        }
                        catch (Exception e)
                        {
                            Client.Log(e.Message + " - remember to fix this later instead of avoiding the problem.");
                        }
                    }

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
                    //ugly hack
                    var user = pres.From.Resource;
                    

                    int ProfileIcon = 0;
                    var x = new GroupChatPlayer();
                    bool exists = false;
                    foreach (var m in from GroupChatPlayer m in ParticipantList.Items where (m).SName.Content.ToString() == user select m)
                    {
                        x = m;
                        exists = true;
                    }
                    x.SName.Content = user;
                    if (pres.Status == null)
                        pres.Status = "<profileIcon>3</profileIcon>";
                    ProfileIcon = Regex.Split(pres.Status, "<profileIcon>")[1].Split(new[] { "</profileIcon>" }, StringSplitOptions.None)[0].ToInt();
                    
                    x.SIcon.Source = new BitmapImage(Client.GetIconUri(ProfileIcon));
                    if (!exists)
                    {
                        ParticipantList.Items.Add(x);
                        var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                        {
                            Text = user + " joined the room." + Environment.NewLine
                        };
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("PrimaryHueMidBrush"));
                    }

                    ParticipantList.Items.Refresh();
                }));
            }
            
        }

        public string ChatId { get; set; }
        public string GroupTitle { get; set; }


        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            userClient.XmppConnection.OnMessage -= XmppConnection_OnMessage;
            userClient.XmppConnection.OnPresence -= XmppConnection_OnPresence;
            newRoom.JoinRoom(new Jid(ChatId), userClient.LoginPacket.AllSummonerData.Summoner.Name);
            Client.ClearMainGrid(typeof (GroupChatItem));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            userClient.XmppConnection.OnMessage -= XmppConnection_OnMessage;
            userClient.XmppConnection.OnPresence -= XmppConnection_OnPresence;
            newRoom.LeaveRoom(new Jid(ChatId), userClient.LoginPacket.AllSummonerData.Summoner.Name);
            Client.ClearMainGrid(typeof (GroupChatItem));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ChatTextBox.Text))
                return;

            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = userClient.LoginPacket.AllSummonerData.Summoner.Name + ": "
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("PrimaryHueMidBrush"));

            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = ChatTextBox.Text + Environment.NewLine
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, FindResource("MaterialDesignBody"));

            userClient.XmppConnection.Send(new Message(new Jid(roomName), MessageType.groupchat, ChatTextBox.Text));
            ChatTextBox.Text = string.Empty;
            ChatText.ScrollToEnd();
        }
    }
}