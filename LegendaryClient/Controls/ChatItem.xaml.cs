#region

using System;
using System.Linq;
using System.Threading;
using System.Windows;
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
            {
                return;
            }

            var chatItem = Client.AllPlayers[msg.From.User];
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                if ((string) Client.ChatItem.PlayerLabelName.Content == chatItem.Username)
                {
                    Update();
                }
            }));
        }

        public void Update()
        {
            ChatText.Items.Clear();
            var tempItem =
                (from x in Client.AllPlayers
                    where x.Value.Username == (string) Client.ChatItem.PlayerLabelName.Content
                    select x.Value).FirstOrDefault();

            if (tempItem != null)
            {
                foreach (var x in tempItem.Messages.ToArray())
                {
                    var message = x.Split('|');
                    var innerChatItem = new InnerChatItem();
                    if (message[0] == tempItem.Username)
                    {
                        innerChatItem.SummonerLabel.Content = tempItem.Username;
                        innerChatItem.SummonerLabel.Foreground = Brushes.Gold;
                    }
                    else
                    {
                        innerChatItem.SummonerLabel.Content = message[0];
                        innerChatItem.SummonerLabel.Foreground = Brushes.SteelBlue;
                    }
                    innerChatItem.MessageLabel.Content = x.Replace(message[0] + "|", string.Empty);
                    innerChatItem.TimeLabel.Content = DateTime.Now.ToString("h:mm");
                    innerChatItem.TimeLabel.Foreground = (ChatText.Items.Count%2 != 0)
                        ? new SolidColorBrush(Color.FromArgb(255, 37, 37, 37))
                        : new SolidColorBrush(Color.FromArgb(255, 77, 77, 77));
                    innerChatItem.Background = (ChatText.Items.Count%2 == 0)
                        ? new SolidColorBrush(Color.FromArgb(255, 37, 37, 37))
                        : new SolidColorBrush(Color.FromArgb(0, 77, 77, 77));

                    if (ChatText.Items.Count != 0 &&
                        ((InnerChatItem) ChatText.Items[ChatText.Items.Count - 1]).SummonerLabel.Content ==
                        innerChatItem.SummonerLabel.Content)
                    {
                        ((InnerChatItem) ChatText.Items[ChatText.Items.Count - 1]).MessageLabel.Content +=
                            Environment.NewLine + innerChatItem.MessageLabel.Content;
                    }
                    else
                    {
                        ChatText.Items.Add(innerChatItem);
                    }
                }
            }
            if (ChatText.Items.Count != 0)
            {
                ChatText.ScrollIntoView(ChatText.Items[ChatText.Items.Count - 1]);
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(ChatTextBox.Text))
            {
                return;
            }

            var innerChatItem = new InnerChatItem
            {
                SummonerLabel =
                {
                    Content = Client.LoginPacket.AllSummonerData.Summoner.Name,
                    Foreground = Brushes.SteelBlue
                },
                MessageLabel =
                {
                    Content = ChatTextBox.Text
                },
                TimeLabel =
                {
                    Content = DateTime.Now.ToString("h:mm"),
                    Foreground = (ChatText.Items.Count%2 != 0)
                        ? new SolidColorBrush(Color.FromArgb(255, 37, 37, 37))
                        : new SolidColorBrush(Color.FromArgb(255, 77, 77, 77))
                },
                Background = (ChatText.Items.Count%2 == 0)
                    ? new SolidColorBrush(Color.FromArgb(255, 37, 37, 37))
                    : new SolidColorBrush(Color.FromArgb(0, 77, 77, 77))
            };

            if (ChatText.Items.Count != 0 &&
                ((InnerChatItem) ChatText.Items[ChatText.Items.Count - 1]).SummonerLabel.Content ==
                innerChatItem.SummonerLabel.Content)
            {
                ((InnerChatItem) ChatText.Items[ChatText.Items.Count - 1]).MessageLabel.Content +=
                    Environment.NewLine + innerChatItem.MessageLabel.Content;
            }
            else
            {
                ChatText.Items.Add(innerChatItem);
            }

            ChatPlayerItem tempItem = null;
            var jid = string.Empty;
            foreach (
                var x in
                    Client.AllPlayers.Where(x => x.Value.Username == (string) Client.ChatItem.PlayerLabelName.Content))
            {
                tempItem = x.Value;
                jid = x.Key + "@pvp.net";

                break;
            }
            if (tempItem != null)
            {
                tempItem.Messages.Add(Client.LoginPacket.AllSummonerData.Summoner.Name + "|" + ChatTextBox.Text);
            }

            if (ChatText.Items.Count != 0)
            {
                ChatText.ScrollIntoView(ChatText.Items[ChatText.Items.Count - 1]);
            }

            Client.ChatClient.Message(jid, Environment.NewLine + ChatTextBox.Text);
            ChatTextBox.Text = string.Empty;
        }
    }
}