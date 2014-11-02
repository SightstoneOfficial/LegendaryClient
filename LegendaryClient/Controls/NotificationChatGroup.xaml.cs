using LegendaryClient.Logic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Media.Animation;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for NotificationChatGroup.xaml
    /// </summary>
    public partial class NotificationChatGroup : UserControl
    {
        public string GroupTitle;

        public NotificationChatGroup()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.MainGrid.Children.Remove(Client.GroupChatItems.Single(i => i.GroupTitle == GroupTitle));
            Client.GroupChatItems.Remove(Client.GroupChatItems.Single(i => i.GroupTitle == GroupTitle));
            Client.ChatListView.Items.Remove(this);
        }
    }
}