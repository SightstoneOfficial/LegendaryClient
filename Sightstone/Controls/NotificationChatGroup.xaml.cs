#region

using System.Linq;
using System.Windows;
using Sightstone.Logic;
using Sightstone.Logic.MultiUser;

#endregion

namespace Sightstone.Controls
{
    /// <summary>
    ///     Interaction logic for NotificationChatGroup.xaml
    /// </summary>
    public partial class NotificationChatGroup
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