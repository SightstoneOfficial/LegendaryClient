using LegendaryClient.Logic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using LegendaryClient.Logic.Riot;
using agsXMPP.protocol.client;
using agsXMPP;
using agsXMPP.protocol.x.muc;
using agsXMPP.Collections;
using LegendaryClient.Logic.Riot.Platform;

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
            newRoom.AcceptDefaultConfiguration(new Jid(ChatId));
            roomName = ChatId;
            newRoom.JoinRoom(new Jid(ChatId), Client.LoginPacket.AllSummonerData.Summoner.Name);
        }

        void XmppConnection_OnMessage(object sender, Message msg)
        {
            if (msg.From.Bare == roomName)
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
                            var user = Client.GetUserFromJid(jid);

                            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                            {
                                Text = user + " left the room." + Environment.NewLine
                            };
                            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                           
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
                            var user = Client.GetUserFromJid(jid);

                            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                            {
                                Text = user + " joined the room." + Environment.NewLine
                            };
                            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);

                        }
                        catch (Exception e)
                        {
                            Client.Log(e.Message + " - remember to fix this later instead of avoiding the problem.");
                        }
                    }

                    int ProfileIcon = 0;
                    var x = new GroupChatPlayer();
                    foreach (var m in from object m in ParticipantList.Items where ((GroupChatPlayer) m).SName.Content == pres.From.User select m)
                    {
                        x = (GroupChatPlayer)m;
                    }
                    x.SName.Content = pres.From.User;
                    if (pres.Status == null)
                        pres.Status = "<profileIcon>3</profileIcon>";
                    using (XmlReader reader = XmlReader.Create(new StringReader(pres.Status)))
                    {
                        try
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsStartElement() || reader.IsEmptyElement)
                                    continue;

                                #region Parse Presence

                                switch (reader.Name)
                                {
                                    case "profileIcon":
                                        reader.Read();
                                        ProfileIcon = Convert.ToInt32(reader.Value);
                                        break;
                                }

                                #endregion Parse Presence
                            }
                        }
                        catch (Exception e)
                        {
                            Client.Log(e.Message + " - remember to fix this later instead of avoiding the problem.");
                        }
                    }
                    var UriSource = Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon",
                        ProfileIcon + ".png");
                    x.SIcon.Source = Client.GetImage(UriSource);
                    ParticipantList.Items.Add(x);
                    ParticipantList.Items.Refresh();
                }));
            }
            
        }

        public string ChatId { get; set; }
        public string GroupTitle { get; set; }


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