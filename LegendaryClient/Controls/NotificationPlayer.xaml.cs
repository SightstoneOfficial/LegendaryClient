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
using LegendaryClient.Windows;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for NotificationPlayer.xaml
    /// </summary>
    public partial class NotificationPlayer : UserControl
    {
        private PlayerChatBox ChatBox;
        public NotificationPlayer()
        {
            InitializeComponent();
        }

        private void PlayerGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChatPlayerItem playerItem = (ChatPlayerItem)Tag;
            if (ChatBox == null)
            {
                ChatBox = new PlayerChatBox();
                Client.MainGrid.Children.Add(ChatBox);
            }
            ChatBox.Player = playerItem;
            ChatBox.Tag = playerItem;
            ChatBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            ChatBox.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            ChatBox.Margin = new Thickness(this.TransformToAncestor(Client.MainGrid).Transform(new Point(0,0)).X, 0, 0, 40);
            ChatBox.Width = 250;
        }
    }
}
