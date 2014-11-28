using LegendaryClient.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for JoinPublicChat.xaml
    /// </summary>
    public partial class JoinPublicChat : UserControl
    {
        public JoinPublicChat()
        {
            InitializeComponent();
        }

        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            string JID = Client.GetChatroomJID(Client.GetObfuscatedChatroomName(ChatNameTextBox.Text.ToLower(), ChatPrefixes.Public), string.Empty, true);
            GroupChatItem item = new GroupChatItem(JID, ChatNameTextBox.Text);
            NotificationChatGroup ChatGroup = new NotificationChatGroup();
            ChatGroup.Tag = item;
            ChatGroup.GroupTitle = item.GroupTitle;
            ChatGroup.Margin = new Thickness(1, 0, 1, 0);
            ChatGroup.GroupLabelName.Content = item.GroupTitle;
            if (!Client.GroupChatItems.Any(i => i.GroupTitle == ChatNameTextBox.Text))
            {
                Client.ChatListView.Items.Add(ChatGroup);
                Client.GroupChatItems.Add(item);
            }
            Client.ClearNotification(typeof(JoinPublicChat));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.ClearNotification(typeof(JoinPublicChat));
        }
    }
}
