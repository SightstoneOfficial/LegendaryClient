#region

using System.Linq;
using System.Windows;
using LegendaryClient.Logic;

#endregion

namespace LegendaryClient.Controls
{
    /// <summary>
    ///     Interaction logic for JoinPublicChat.xaml
    /// </summary>
    public partial class JoinPublicChat
    {
        public JoinPublicChat()
        {
            InitializeComponent();
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            var jid =
                Client.GetChatroomJID(
                    Client.GetObfuscatedChatroomName(ChatNameTextBox.Text.ToLower(), ChatPrefixes.Public), string.Empty,
                    true);
            var item = new GroupChatItem(jid, ChatNameTextBox.Text);
            var chatGroup = new NotificationChatGroup
            {
                Tag = item,
                GroupTitle = item.GroupTitle,
                Margin = new Thickness(1, 0, 1, 0),
                GroupLabelName = {Content = item.GroupTitle}
            };

            if (Client.GroupChatItems.All(i => i.GroupTitle != ChatNameTextBox.Text))
            {
                Client.ChatListView.Items.Add(chatGroup);
                Client.GroupChatItems.Add(item);
            }
            Client.ClearNotification(typeof (JoinPublicChat));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.ClearNotification(typeof (JoinPublicChat));
        }
    }
}