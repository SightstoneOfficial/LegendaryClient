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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using LegendaryClient.Logic;

namespace LegendaryClient.Controls
{
    /// <summary>
    /// Interaction logic for PlayerChatBox.xaml
    /// </summary>
    public partial class PlayerChatBox : UserControl
    {
        public ChatPlayerItem Player;
        internal System.Timers.Timer UpdateTimer;

        public PlayerChatBox()
        {
            InitializeComponent();
            UpdateTimer = new System.Timers.Timer(1000);
            UpdateTimer.Elapsed += new System.Timers.ElapsedEventHandler(UpdateChat);
            UpdateTimer.Enabled = true;
            UpdateTimer.Start();
            UpdateChat(null, null);
        }

        void UpdateChat(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                MessagesTextBox.Text = "";
                foreach (string message in Player.Messages)
                {
                    MessagesTextBox.Text += String.Format("[{0}] {1}", DateTime.Now.ToShortTimeString(), message + Environment.NewLine);
                }
            }));
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            Client.SendMessage(Player.Id + "@pvp.net", SendMessageTextBox.Text);
            Player.Messages.Add(Client.LoginPacket.AllSummonerData.Summoner.Name + ": " + SendMessageTextBox.Text);
            SendMessageTextBox.Text = "";
        }
    }
}
