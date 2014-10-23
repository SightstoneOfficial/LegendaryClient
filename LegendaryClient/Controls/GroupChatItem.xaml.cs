using LegendaryClient.Logic;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for ChatItem.xaml
    /// </summary>
    public partial class GroupChatItem : UserControl
    {
        public GroupChatItem()
        {
            InitializeComponent();
            Client.ChatClient.OnMessage += GroupChatClient_OnMessage;
        }

        public void GroupChatClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            if (Client.AllPlayers.ContainsKey(msg.From.User) && !String.IsNullOrWhiteSpace(msg.Body))
            {
                ChatPlayerItem chatItem = Client.AllPlayers[msg.From.User];
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    if ((string)Client.ChatItem.PlayerLabelName.Content == chatItem.Username)
                    {
                        Update();
                    }
                }));
            }
        }

        public void Update()
        {
            ChatText.Document.Blocks.Clear();
            ChatPlayerItem tempItem = null;
            foreach (KeyValuePair<string, ChatPlayerItem> x in Client.AllPlayers)
            {
                if (x.Value.Username == (string)Client.ChatItem.PlayerLabelName.Content)
                {
                    tempItem = x.Value;
                    break;
                }
            }

            foreach (string x in tempItem.Messages.ToArray())
            {
                string[] Message = x.Split('|');
                TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                if (Message[0] == tempItem.Username)
                {
                    tr.Text = tempItem.Username + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Gold);
                }
                else
                {
                    tr.Text = Message[0] + ": ";
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.SteelBlue);
                }
                tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
                tr.Text = x.Replace(Message[0] + "|", "") + Environment.NewLine;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);
            }

            ChatText.ScrollToEnd();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Client.MainGrid.Children.Remove(Client.ChatItem);
            Client.ChatClient.OnMessage -= Client.ChatItem.ChatClient_OnMessage;
            Client.ChatItem = null;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NotificationChatPlayer tempPlayer = null;

            foreach (NotificationChatPlayer x in Client.ChatListView.Items)
            {
                if (x.PlayerLabelName.Content == Client.ChatItem.PlayerLabelName.Content)
                {
                    tempPlayer = x;
                    break;
                }
            }

            Client.MainGrid.Children.Remove(Client.ChatItem);
            Client.ChatClient.OnMessage -= Client.ChatItem.ChatClient_OnMessage;
            Client.ChatItem = null;

            Client.ChatListView.Items.Remove(tempPlayer);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            TextRange tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            tr.Text = Client.LoginPacket.AllSummonerData.Summoner.Name + ": ";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.SteelBlue);
            tr = new TextRange(ChatText.Document.ContentEnd, ChatText.Document.ContentEnd);
            tr.Text = ChatTextBox.Text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.White);

            ChatPlayerItem tempItem = null;
            string JID = "";
            foreach (KeyValuePair<string, ChatPlayerItem> x in Client.AllPlayers)
            {
                if (x.Value.Username == (string)Client.ChatItem.PlayerLabelName.Content)
                {
                    tempItem = x.Value;
                    JID = x.Key + "@pvp.net";
                    break;
                }
            }
            tempItem.Messages.Add(Client.LoginPacket.AllSummonerData.Summoner.Name + "|" + ChatTextBox.Text);
            ChatText.ScrollToEnd();

            Client.ChatClient.Message(JID, Environment.NewLine + ChatTextBox.Text);

            ChatTextBox.Text = "";
        }
    }
}