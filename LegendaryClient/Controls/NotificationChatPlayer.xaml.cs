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
            Client.ChatListView.Items.Remove(this);
        }
    }
}
