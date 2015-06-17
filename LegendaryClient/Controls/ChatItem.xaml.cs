using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using agsXMPP.protocol.client;
using agsXMPP;

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for ChatItem.xaml
    /// </summary>
    public partial class ChatItem
    {
        public ChatItem()
        {
            InitializeComponent();

            Client.XmppConnection.OnMessage += XmppConnection_OnMessage;
        }

        public void XmppConnection_OnMessage(object sender, Message msg)
        {
            if (!Client.AllPlayers.ContainsKey(msg.From.User) || string.IsNullOrWhiteSpace(msg.Body))
                return;

            var chatItem = Client.AllPlayers[msg.From.User];
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if ((string)Client.ChatItem.PlayerLabelName.Content == chatItem.Username)
                    Update();
            }));
        }

        public void Update()
        {
            ChatText.Document.Blocks.Clear();
            var tempItem =
                (from x in Client.AllPlayers
                 where x.Value.Username == (string)Client.ChatItem.PlayerLabelName.Content
                 select x.Value).FirstOrDefault();

            if (tempItem != null)
                foreach (var x in tempItem.Messages.ToArray())
                {
                    var message = x.Split('|');
                    var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    if (message[0] == tempItem.Username)
                    {
                        tr.Text = DateTime.Now.ToString("[HH:mm] ") + tempItem.Username + ": ";
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Turquoise);
                    }
                    else
                    {
                        tr.Text = DateTime.Now.ToString("[HH:mm] ") + message[0] + ": ";
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);
                    }
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                    {
                        Text = x.Replace(message[0] + "|", string.Empty) + Environment.NewLine
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }

            ChatText.ScrollToEnd();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ChatTextBox.Text))
                return;

            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = DateTime.Now.ToString("[HH:mm] ") + Client.LoginPacket.AllSummonerData.Summoner.Name + ": "
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Yellow);

            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = ChatTextBox.Text + Environment.NewLine
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

            ChatPlayerItem tempItem = null;
            var Jid = string.Empty;
            foreach (
                var x in
                    Client.AllPlayers.Where(x => x.Value.Username == (string)Client.ChatItem.PlayerLabelName.Content))
            {
                tempItem = x.Value;
                Jid = x.Key + "@pvp.net";

                break;
            }
            if (tempItem != null)
                tempItem.Messages.Add(Client.LoginPacket.AllSummonerData.Summoner.Name + "|" + ChatTextBox.Text);

            ChatText.ScrollToEnd();
            Client.XmppConnection.Send(new Message(new Jid(Jid), ChatTextBox.Text));
            ChatTextBox.Text = string.Empty;
        }
    }
}