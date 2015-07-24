using Sightstone.Logic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using RtmpSharp.Messaging;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for InGame.xaml
    /// </summary>
    public partial class InGame
    {
        static UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
        public InGame(bool start = false)
        {
            InitializeComponent();
            if (UserClient.GameType == "PRACTICE_GAME")
                QuitButton.Visibility = Visibility.Visible;

            if (start)
            {
                Process[] lol = Process.GetProcessesByName("League of Legends.exe");
                if (lol.Length == 0)
                    Client.LaunchGame(UserClient.CurrentGame.ServerIp, UserClient.CurrentGame.ServerPort.ToString(), UserClient.CurrentGame.EncryptionKey,
                        UserClient.CurrentGame.SummonerId.ToString(), UserClient.CurrentGame.SummonerName, UserClient.Region);
            }

            UserClient.RiotConnection.MessageReceived += Update_OnMessageReceived;
            UserClient.IsInGame = true;
            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Reconnect Page";
        }

        private void Update_OnMessageReceived(object sender, MessageReceivedEventArgs message)
        {
            if (message.Body.GetType() != typeof(GameDTO))
                return;

            if (((GameDTO)message.Body).GameState != "TERMINATED")
                return;

            UserClient.GameStatus = "outOfGame";
            UserClient.SetChatHover();
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Client.ReturnButton.Visibility = Visibility.Hidden;
                UserClient.IsInGame = false;
                UserClient.RiotConnection.MessageReceived -= Update_OnMessageReceived;
                Client.SwitchPage(Client.MainPage);
                Client.ClearPage(typeof(InGame));
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Client.LaunchGame(UserClient.CurrentGame.ServerIp, UserClient.CurrentGame.ServerPort.ToString(), UserClient.CurrentGame.EncryptionKey,
                        UserClient.CurrentGame.SummonerId.ToString(), UserClient.CurrentGame.SummonerName, UserClient.Region);
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            UserClient.GameStatus = "outOfGame";
            UserClient.SetChatHover();
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Client.ReturnButton.Visibility = Visibility.Hidden;
                UserClient.IsInGame = false;
                UserClient.RiotConnection.MessageReceived -= Update_OnMessageReceived;
                Client.SwitchPage(Client.MainPage);
                Client.ClearPage(typeof(InGame));
            }));
        }
    }
}