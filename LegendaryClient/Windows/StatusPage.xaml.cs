#region

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using jabber.protocol.client;
using LegendaryClient.Controls;
using LegendaryClient.Logic;
using LegendaryClient.Properties;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for StatusPage.xaml
    /// </summary>
    public partial class StatusPage
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
        private void ChatClient_OnMessage(object sender, Message msg)
        {
            //If is special message, don't show popup
            if (msg.Subject != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var subject = (ChatSubjects)Enum.Parse(typeof(ChatSubjects), msg.Subject, true);

                    if ((subject == ChatSubjects.PRACTICE_GAME_INVITE ||
                         subject == ChatSubjects.GAME_INVITE) &&
                        Client.NotificationContainer.Visibility != Visibility.Visible)
                    {
                        NotificationButton.Content = ".";
                    }
                }));
                return;
            }

            if (!Client.AllPlayers.ContainsKey(msg.From.User) || String.IsNullOrWhiteSpace(msg.Body))
                return;

            ChatPlayerItem chatItem = Client.AllPlayers[msg.From.User];
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                NotificationChatPlayer player =
                    ChatListView.Items.Cast<object>()
                        .Where(i => i.GetType() == typeof(NotificationChatPlayer))
                        .Cast<NotificationChatPlayer>()
                        .FirstOrDefault(x => x.PlayerName == chatItem.Username);

                if (player == null)
                {
                    player = new NotificationChatPlayer
                    {
                        Tag = chatItem,
                        PlayerName = chatItem.Username,
                        Margin = new Thickness(1, 0, 1, 0),
                        PlayerLabelName = { Content = chatItem.Username }
                    };
                    Client.ChatListView.Items.Add(player);
                }

                if (Client.ChatItem != null)
                {
                    if ((string)Client.ChatItem.PlayerLabelName.Content != chatItem.Username)
                    {
                        player.BlinkRectangle.Visibility = Visibility.Visible;
                    }
                }
                else
                    player.BlinkRectangle.Visibility = Visibility.Visible;
            }));
        }

        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.ChatContainer.Visibility == Visibility.Hidden)
            {
                if (Client.Dev)
                {
                    Client.FriendList.RankedStatus.Visibility = Visibility.Visible;
                    Client.FriendList.FriendsList.Margin = new Thickness(0, 97, 0, 40);
                }
                else
                    Client.FriendList.RankedStatus.Visibility = Visibility.Hidden;
                Client.ChatContainer.Visibility = Visibility.Visible;
                Client.NotificationContainer.Visibility = Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 260, 50);
            }
            else
            {
                Client.ChatContainer.Visibility = Visibility.Hidden;
                Client.NotificationContainer.Visibility = Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 10, 50);
            }
        }

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.NotificationContainer.Visibility == Visibility.Hidden)
            {
                Client.NotificationContainer.Visibility = Visibility.Visible;
                Client.ChatContainer.Visibility = Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 260, 50);
            }
            else
            {
                Client.NotificationContainer.Visibility = Visibility.Hidden;
                Client.ChatContainer.Visibility = Visibility.Hidden;
                Client.NotificationOverlayContainer.Margin = new Thickness(0, 0, 10, 50);
            }
        }

        private void JoinChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (
                Client.NotificationGrid.Children.Cast<object>()
                    .Any(element => element.GetType() == typeof(JoinPublicChat)))
            {
                Client.ClearNotification(typeof(JoinPublicChat));

                return;
            }
            var pop = new JoinPublicChat
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            Client.NotificationGrid.Children.Add(pop);
        }

        public void ChatListView_ItemClicked(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                var tItem = sender as ListViewItem;
                if (tItem == null || ChatListView.SelectedItem == null)
                    return;

                tItem.IsSelected = true;
                if (ChatListView.SelectedItem.GetType() == typeof(NotificationChatPlayer))
                {
                    if (Client.GroupIsShown)
                    {
                        foreach (GroupChatItem groupItem in Client.GroupChatItems)
                            Client.MainGrid.Children.Remove(groupItem);
                        Client.GroupIsShown = false;
                    }
                    var item = (NotificationChatPlayer)ChatListView.SelectedItem;
                    if (Client.ChatItem == null)
                    {
                        Client.ChatItem = new ChatItem();
                        Client.MainGrid.Children.Add(Client.ChatItem);
                        Client.PlayerChatIsShown = true;
                    }
                    else
                    {
                        var currentName = (string)Client.ChatItem.PlayerLabelName.Content;
                        Client.MainGrid.Children.Remove(Client.ChatItem);
                        Client.ChatClient.OnMessage -= Client.ChatItem.ChatClient_OnMessage;
                        Client.ChatItem = null;
                        Client.PlayerChatIsShown = false;
                        if (currentName != (string)item.PlayerLabelName.Content)
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

                    item.BlinkRectangle.Visibility = Visibility.Hidden;

                    Panel.SetZIndex(Client.ChatItem, 3);

                    Client.ChatItem.PlayerLabelName.Content = item.PlayerLabelName.Content;

                    Client.ChatItem.Update();

                    Client.ChatItem.HorizontalAlignment = HorizontalAlignment.Left;
                    Client.ChatItem.VerticalAlignment = VerticalAlignment.Bottom;

                    Point relativePoint = item.TransformToAncestor(Client.MainWin).Transform(new Point(0, 0));

                    Client.ChatItem.Margin = new Thickness(relativePoint.X, 0, 0, 40);
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
                        var item = (NotificationChatGroup)ChatListView.SelectedItem;
                        Client.CurrentGroupChatItem =
                            Client.GroupChatItems.Single(i => i.GroupTitle == (string)item.GroupLabelName.Content);
                        if (!Client.MainGrid.Children.Contains(Client.CurrentGroupChatItem))
                            Client.MainGrid.Children.Add(Client.CurrentGroupChatItem);

                        Client.CurrentGroupChatItem.HorizontalAlignment = HorizontalAlignment.Left;
                        Client.CurrentGroupChatItem.VerticalAlignment = VerticalAlignment.Bottom;
                        Point relativePoint = item.TransformToAncestor(Client.MainWin).Transform(new Point(0, 0));

                        Client.CurrentGroupChatItem.Margin = new Thickness(relativePoint.X, 0, 0, 40);
                    }
                    else
                    {
                        foreach (GroupChatItem item in Client.GroupChatItems)
                            Client.MainGrid.Children.Remove(item);

                        if (Client.CurrentGroupChatItem !=
                            Client.GroupChatItems.Single(
                                i =>
                                {
                                    var notificationChatGroup = ChatListView.SelectedItem as NotificationChatGroup;
                                    return notificationChatGroup != null && i.GroupTitle ==
                                           (string)
                                               notificationChatGroup.GroupLabelName.Content;
                                }))
                        {
                            Client.CurrentGroupChatItem =
                                Client.GroupChatItems.Single(
                                    i =>
                                    {
                                        var notificationChatGroup = ChatListView.SelectedItem as NotificationChatGroup;
                                        return notificationChatGroup != null && i.GroupTitle ==
                                               (string)
                                                   notificationChatGroup.GroupLabelName
                                                       .Content;
                                    });
                            Client.MainGrid.Children.Add(Client.CurrentGroupChatItem);
                            Client.CurrentGroupChatItem.HorizontalAlignment = HorizontalAlignment.Left;
                            Client.CurrentGroupChatItem.VerticalAlignment = VerticalAlignment.Bottom;
                            var chatGroup = ChatListView.SelectedItem as NotificationChatGroup;
                            if (chatGroup != null)
                            {
                                Point relativePoint =
                                    chatGroup.TransformToAncestor(
                                        Client.MainWin).Transform(new Point(0, 0));

                                Client.CurrentGroupChatItem.Margin = new Thickness(relativePoint.X, 0, 0, 40);
                            }
                            Client.GroupIsShown = !Client.GroupIsShown;
                        }
                    }
                    Client.ChatItem = null;
                    Client.GroupIsShown = !Client.GroupIsShown;
                }
            }));
        }

        public void Change()
        {
            string y = Settings.Default.Theme;
            var bc = new BrushConverter();
            if (y.Contains("Steel"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FF141414");
            else if (y.Contains("Blue"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FF1585B5");
            else if (y.Contains("Red"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FFA01414");
            else if (y.Contains("Green"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FF2DA014");
            else if (y.Contains("Purple"))
                BackBar.Background = (Brush)bc.ConvertFrom("#FF5A14A0");

            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(y)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }
    }
}