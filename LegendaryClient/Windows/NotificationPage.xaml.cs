using LegendaryClient.Controls;
using LegendaryClient.Logic;
using System.Windows;
using System.Windows.Controls;

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
                    Client.ChatItem = null;
                    ChatListView.SelectedIndex = -1;
                    return;
                }

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