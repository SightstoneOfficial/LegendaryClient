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
using LegendaryClient.Logic;
using LegendaryClient.Controls;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for ChatPage.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        public ChatPage()
        {
            InitializeComponent();
            ChatPlayer testPlayer = new ChatPlayer();
            ChatPlayer testPlayer1 = new ChatPlayer();
            ChatPlayer testPlayer2 = new ChatPlayer();
            ChatPlayer testPlayer3 = new ChatPlayer();

            ChatListView.Items.Add(testPlayer);
            ChatListView.Items.Add(testPlayer1);
            ChatListView.Items.Add(testPlayer2);
            ChatListView.Items.Add(testPlayer3);
        }
    }
}
