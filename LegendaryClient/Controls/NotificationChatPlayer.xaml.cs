using LegendaryClient.Logic;
using System.Windows;
using System.Windows.Controls;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for NotificationChatPlayer.xaml
    /// </summary>
    public partial class NotificationChatPlayer : UserControl
    {
        public NotificationChatPlayer()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.ChatItem != null)
            {
                Client.MainGrid.Children.Remove(Client.ChatItem);
                Client.ChatItem = null;
            }

            Client.ChatListView.Items.Remove(this);
        }
    }
}