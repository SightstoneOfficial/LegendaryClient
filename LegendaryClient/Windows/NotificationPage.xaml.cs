using LegendaryClient.Logic;
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
    }
}