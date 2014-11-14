using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for StatusPage.xaml
    /// </summary>
    public partial class StatusPage : Page
    {
        public StatusPage()
        {
            InitializeComponent();
            Client.StatusLabel = StatusLabel;
            //Client.StatusGrid = StatusGrid;
            Client.ChatListView = ChatListView;
            Client.ChatClient.OnMessage += ChatClient_OnMessage;
            Change();
        }

        //Blink and add to notification list if messaged
        void ChatClient_OnMessage(object sender, jabber.protocol.client.Message msg)
        {
            //If is special message, don't show popup
            if (msg.Subject != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    ChatSubjects subject = (ChatSubjects)Enum.Parse(typeof(ChatSubjects), msg.Subject, true);

                    if ((subject == ChatSubjects.PRACTICE_GAME_INVITE ||
                        subject == ChatSubjects.GAME_INVITE) &&
                        Client.NotificationContainer.Visibility != System.Windows.Visibility.Visible)
                    {
                        NotificationButton.Content = ".";
                    }
                }));
                return;
            }

            if (Client.AllPlayers.ContainsKey(msg.From.User) && !String.IsNullOrWhiteSpace(msg.Body))
            {
                ChatPlayerItem chatItem = Client.AllPlayers[msg.From.User];
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    NotificationChatPlayer player = null;
                    foreach (NotificationChatPlayer x in ChatListView.Items.Cast<object>().Where(i => i.GetType() == typeof(NotificationChatPlayer)))
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
                Client.NotificationContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 260, 50);
            }
            else
            {
                Client.ChatContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 10, 50);
            }
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.NotificationContainer.Visibility == System.Windows.Visibility.Hidden)
            {
                Client.NotificationContainer.Visibility = System.Windows.Visibility.Visible;
                Client.ChatContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 260, 50);
            }
            else
            {
                Client.NotificationContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.ChatContainer.Visibility = System.Windows.Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 10, 50);
            }
        }

        private void JoinChatButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var element in Client.NotificationGrid.Children)
                if (element.GetType() == typeof(JoinPublicChat))
                {
                    Client.ClearNotification(typeof(JoinPublicChat));
                    return;
                }
            JoinPublicChat pop = new JoinPublicChat();
            pop.HorizontalAlignment = HorizontalAlignment.Right;
            pop.VerticalAlignment = VerticalAlignment.Bottom;
            Client.NotificationGrid.Children.Add(pop);
        }

        public void ChatListView_ItemClicked(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var tItem = sender as ListViewItem;
                if (tItem != null && ChatListView.SelectedItem != null)
                {
                    tItem.IsSelected = true;
                    if (ChatListView.SelectedItem.GetType() == typeof(NotificationChatPlayer))
                    {
                        if (Client.GroupIsShown)
                        {
                            foreach (GroupChatItem groupItem in Client.GroupChatItems)
                                Client.MainGrid.Children.Remove(groupItem);
                            Client.GroupIsShown = false;
                        }
                        NotificationChatPlayer item = (NotificationChatPlayer)ChatListView.SelectedItem;
                        if (Client.ChatItem == null)
                        {
                            Client.ChatItem = new ChatItem();
                            Client.MainGrid.Children.Add(Client.ChatItem);
                            Client.PlayerChatIsShown = true;
                        }
                        else
                        {
                            string CurrentName = (string)Client.ChatItem.PlayerLabelName.Content;
                            Client.MainGrid.Children.Remove(Client.ChatItem);
                            Client.ChatClient.OnMessage -= Client.ChatItem.ChatClient_OnMessage;
                            Client.ChatItem = null;
                            Client.PlayerChatIsShown = false;
                            if (CurrentName != (string)item.PlayerLabelName.Content)
                            {
                                Client.ChatItem = new ChatItem();
                                Client.MainGrid.Children.Add(Client.ChatItem);
                                Client.PlayerChatIsShown = true;
                            }
                            else
                            {
                                Client.PlayerChatIsShown = false;
                                return;
                            }
                        }

                        item.BlinkRectangle.Visibility = System.Windows.Visibility.Hidden;

                        Panel.SetZIndex(Client.ChatItem, 3);

                        Client.ChatItem.PlayerLabelName.Content = item.PlayerLabelName.Content;

                        Client.ChatItem.Update();

                        Client.ChatItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                        Client.ChatItem.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;

                        Point relativePoint = item.TransformToAncestor(Client.MainWin).Transform(new Point(0, 0));

                        Client.ChatItem.Margin = new System.Windows.Thickness(relativePoint.X, 0, 0, 40);
                    }
                    else
                    {
                        if (Client.PlayerChatIsShown)
                        {
                            Client.MainGrid.Children.Remove(Client.ChatItem);
                            Client.PlayerChatIsShown = false;
                        }
                        if (!Client.GroupIsShown)
                        {
                            NotificationChatGroup item = (NotificationChatGroup)ChatListView.SelectedItem;
                            Client.CurrentGroupChatItem = Client.GroupChatItems.Single(i => i.GroupTitle == (string)item.GroupLabelName.Content);
                            if (!Client.MainGrid.Children.Contains(Client.CurrentGroupChatItem))
                                Client.MainGrid.Children.Add(Client.CurrentGroupChatItem);
                            Client.CurrentGroupChatItem.HorizontalAlignment = HorizontalAlignment.Left;
                            Client.CurrentGroupChatItem.VerticalAlignment = VerticalAlignment.Bottom;
                            Point relativePoint = item.TransformToAncestor(Client.MainWin).Transform(new Point(0, 0));

                            Client.CurrentGroupChatItem.Margin = new System.Windows.Thickness(relativePoint.X, 0, 0, 40);
                        }
                        else
                        {
                            foreach (GroupChatItem item in Client.GroupChatItems)
                                Client.MainGrid.Children.Remove(item);
                            if (Client.CurrentGroupChatItem != Client.GroupChatItems.Single(i => i.GroupTitle == (string)(ChatListView.SelectedItem as NotificationChatGroup).GroupLabelName.Content))
                            {
                                Client.CurrentGroupChatItem = Client.GroupChatItems.Single(i => i.GroupTitle == (string)(ChatListView.SelectedItem as NotificationChatGroup).GroupLabelName.Content);
                                Client.MainGrid.Children.Add(Client.CurrentGroupChatItem);
                                Client.CurrentGroupChatItem.HorizontalAlignment = HorizontalAlignment.Left;
                                Client.CurrentGroupChatItem.VerticalAlignment = VerticalAlignment.Bottom;
                                Point relativePoint = (ChatListView.SelectedItem as NotificationChatGroup).TransformToAncestor(Client.MainWin).Transform(new Point(0, 0));

                                Client.CurrentGroupChatItem.Margin = new System.Windows.Thickness(relativePoint.X, 0, 0, 40);
                                Client.GroupIsShown = !Client.GroupIsShown;
                            }
                        }
                        Client.ChatItem = null;
                        Client.GroupIsShown = !Client.GroupIsShown;
                    }
                }
            }));
        }
        public void Change()
        {
            bool x = Properties.Settings.Default.DarkTheme;
            string y = Properties.Settings.Default.Theme;
            var bc = new BrushConverter();
            if (y.Contains("Steel"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FF141414");
            else if (y.Contains("Blue"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FF1EA0AF");
            else if (y.Contains("Red"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FFA01414");
            else if (y.Contains("Green"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FF2DA014");
            else if (y.Contains("Purple"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FF5A14A0");
        }
    }
}