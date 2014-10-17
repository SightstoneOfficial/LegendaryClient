using LegendaryClient.Logic;
using PVPNetConnect.RiotObjects.Platform;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for InGame.xaml
    /// </summary>
    public partial class InGame : Page
    {
        public InGame()
        {
            InitializeComponent();
            Client.PVPNet.OnMessageReceived += Update_OnMessageReceived;
        }

        private void Update_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() == typeof(GameDTO))
            {
                if (((GameDTO)message).GameState == "TERMINATED")
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        Client.SwitchPage(new MainPage());
                    }));
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Client.LaunchGame();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new InGameHomeOverlay());
        }
    }
}
