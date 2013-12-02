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
            NotificationChatPlayer item = (NotificationChatPlayer)ChatListView.SelectedItem;
            if (Client.ChatItem == null)
            {
                Client.ChatItem = new ChatItem();
                Client.MainGrid.Children.Add(Client.ChatItem);
            }

            Client.ChatItem.PlayerLabelName.Content = item.PlayerLabelName.Content;

            Client.ChatItem.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            Client.ChatItem.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            
            Point relativePoint = item.TransformToAncestor(Client.MainWin).Transform(new Point(0, 0));

            Client.ChatItem.Margin = new System.Windows.Thickness(relativePoint.X, 0, 0, 50);
        }
    }
}