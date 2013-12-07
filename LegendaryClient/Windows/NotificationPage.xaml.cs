using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for NotificationPage.xaml
    /// </summary>
    public partial class NotificationPage : Page
    {
        public NotificationPage()
        {
            InitializeComponent();
            Client.StatusLabel = StatusLabel;
            Client.ChatListView = ChatListView;
            Client.ChatClient.OnMessage += ChatClient_OnMessage;
        }

        //Blink and add to notification list if messaged
        void ChatClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            if (Client.AllPlayers.ContainsKey(msg.From.User) && !String.IsNullOrWhiteSpace(msg.Body))
            {
                ChatPlayerItem chatItem = Client.AllPlayers[msg.From.User];
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    NotificationChatPlayer player = null;
                    foreach (NotificationChatPlayer x in ChatListView.Items)
                    {
                        if (x.PlayerName == chatItem.Username)
                        {
                            player = x;
                            break;
                        }
                    }

                    if (player == null)
                    {
                        player = new NotificationChatPlayer();
                        player.Tag = chatItem;
                        player.PlayerName = chatItem.Username;
                        player.Margin = new Thickness(1, 0, 1, 0);
                        player.PlayerLabelName.Content = chatItem.Username;
                        Client.ChatListView.Items.Add(player);
                    }

                    if (Client.ChatItem != null)
                    {
                        if ((string)Client.ChatItem.PlayerLabelName.Content != chatItem.Username)
                        {
                            player.BlinkRectangle.Visibility = System.Windows.Visibility.Visible;
                        }
                    }
                    else
                    {
                        player.BlinkRectangle.Visibility = System.Windows.Visibility.Visible;
                    }
                }));
            }
        }

        private void ChatButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Client.ChatContainer.Visibility == System.Windows.Visibility.Hidden)
            {
                Client.ChatContainer.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Client.ChatContainer.Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void ChatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatListView.SelectedIndex != -1)
            {
                NotificationChatPlayer item = (NotificationChatPlayer)ChatListView.SelectedItem;
                if (Client.ChatItem == null)
                {
                    Client.ChatItem = new ChatItem();
                    Client.MainGrid.Children.Add(Client.ChatItem);
                }
                else
                {
                    Client.MainGrid.Children.Remove(Client.ChatItem);
                    Client.ChatClient.OnMessage -= Client.ChatItem.ChatClient_OnMessage;
                    Client.ChatItem = null;
                    ChatListView.SelectedIndex = -1;
                    return;
                }

                item.BlinkRectangle.Visibility = System.Windows.Visibility.Hidden;

                Panel.SetZIndex(Client.ChatItem, 3);

                Client.ChatItem.PlayerLabelName.Content = item.PlayerLabelName.Content;

                Client.ChatItem.Update();

                Client.ChatItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                Client.ChatItem.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

                Point relativePoint = item.TransformToAncestor(Client.MainWin).Transform(new Point(0, 0));

                Client.ChatItem.Margin = new System.Windows.Thickness(relativePoint.X, 0, 0, 40);
                ChatListView.SelectedIndex = -1;
            }
        }
    }
}