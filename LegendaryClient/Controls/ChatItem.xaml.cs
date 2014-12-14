#region

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using jabber.protocol.client;
using LegendaryClient.Logic;
using LegendaryClient.Properties;

#endregion

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
            Change();

            Client.ChatClient.OnMessage += ChatClient_OnMessage;
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        public void ChatClient_OnMessage(object sender, Message msg)
        {
            if (!Client.AllPlayers.ContainsKey(msg.From.User) || String.IsNullOrWhiteSpace(msg.Body))
                return;

            ChatPlayerItem chatItem = Client.AllPlayers[msg.From.User];
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if ((string) Client.ChatItem.PlayerLabelName.Content == chatItem.Username)
                    Update();
            }));
        }

        public void Update()
        {
            ChatText.Document.Blocks.Clear();
            ChatPlayerItem tempItem =
                (from x in Client.AllPlayers
                    where x.Value.Username == (string) Client.ChatItem.PlayerLabelName.Content
                    select x.Value).FirstOrDefault();

            if (tempItem != null)
                foreach (string x in tempItem.Messages.ToArray())
                {
                    string[] message = x.Split('|');
                    var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                    if (message[0] == tempItem.Username)
                    {
                        tr.Text = tempItem.Username + ": ";
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gold);
                    }
                    else
                    {
                        tr.Text = message[0] + ": ";
                        tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.SteelBlue);
                    }
                    tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
                    {
                        Text = x.Replace(message[0] + "|", "") + Environment.NewLine
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
                }

            ChatText.ScrollToEnd();
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            var tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": "
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.SteelBlue);

            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd)
            {
                Text = ChatTextBox.Text + Environment.NewLine
            };
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

            ChatPlayerItem tempItem = null;
            string jid = "";
            foreach (
                var x in
                    Client.AllPlayers.Where(x => x.Value.Username == (string) Client.ChatItem.PlayerLabelName.Content))
            {
                tempItem = x.Value;
                jid = x.Key + "@pvp.net";
                break;
            }
            if (tempItem != null)
                tempItem.Messages.Add(Client.LoginPacket.AllSummonerData.Summoner.Name + "|" + ChatTextBox.Text);

            ChatText.ScrollToEnd();
            Client.ChatClient.Message(jid, Environment.NewLine + ChatTextBox.Text);
            ChatTextBox.Text = "";
        }
    }
}