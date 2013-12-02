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
    /// Interaction logic for ChatItem.xaml
    /// </summary>
    public partial class ChatItem : UserControl
    {
        public ChatItem()
        {
            InitializeComponent();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Client.MainGrid.Children.Remove(Client.ChatItem);
            Client.ChatItem = null;
        }
    }
}
