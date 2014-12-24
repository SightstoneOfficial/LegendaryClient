#region

using System;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using LegendaryClient.Logic;
using LegendaryClient.Properties;
using PVPNetConnect.RiotObjects.Platform.Game;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for InGame.xaml
    /// </summary>
    public partial class InGame
    {
        public InGame()
        {
            InitializeComponent();
            Change();

            Client.PVPNet.OnMessageReceived += Update_OnMessageReceived;
            Client.IsInGame = true;
            Client.CurrentPage = this;
            Client.ReturnButton.Visibility = Visibility.Visible;
            Client.ReturnButton.Content = "Return to Reconnect Page";
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void Update_OnMessageReceived(object sender, object message)
        {
            if (message.GetType() != typeof (GameDTO))
                return;

            if (((GameDTO) message).GameState != "TERMINATED")
                return;

            Client.GameStatus = "outOfGame";
            Client.SetChatHover();
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Client.ReturnButton.Visibility = Visibility.Hidden;
                Client.IsInGame = false;
                Client.PVPNet.OnMessageReceived -= Update_OnMessageReceived;
                uiLogic.UpdateMainPage();
                Client.ClearPage(typeof(InGame));
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Client.LaunchGame();
        }
    }
}