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
            string JID = Client.GetChatroomJID(Client.GetObfuscatedChatroomName(ChatNameTextBox.Text, ChatPrefixes.Public), string.Empty, true);
            Client.MainGrid.Children.Add(new GroupChatItem(JID, ChatNameTextBox.Text));
            Client.ClearNotification(typeof(JoinPublicChat));
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Client.ClearNotification(typeof(JoinPublicChat));
        }
    }
}
