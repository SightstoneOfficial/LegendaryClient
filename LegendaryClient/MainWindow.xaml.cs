using LegendaryClient.Logic;
using LegendaryClient.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using PVPNetConnect;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace LegendaryClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Accent Steel = null;

        public MainWindow()
        {
            InitializeComponent();
            Client.ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            Client.InfoLabel = InfoLabel;
            Client.PVPNet = new PVPNetConnection();
            Client.PVPNet.OnError += Client.PVPNet_OnError;

            Steel = new Accent("Steel", new Uri("pack://application:,,,/LegendaryClient;component/Controls/Steel.xaml"));
            if (Properties.Settings.Default.DarkTheme)
            {
                ThemeManager.ChangeTheme(this, Steel, Theme.Dark);
            }

            ChatContainer.Content = new ChatPage().Content;
            NotificationContainer.Content = new NotificationPage().Content;

            Client.Pages = new List<Page>();
            Client.MainGrid = MainGrid;
            Client.Container = Container;
            Client.OverlayContainer = OverlayContainer;
            Client.MainWin = this;
            Client.ChatContainer = ChatContainer;
            Client.NotificationContainer = NotificationContainer;
            Client.SwitchPage(new PatcherPage());
        }

        private void ThemeButton_Click(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.ThemeIsDark)
            {
                ThemeManager.ChangeTheme(this, Steel, Theme.Light);
                Properties.Settings.Default.DarkTheme = false;
                Properties.Settings.Default.Save();
            }
            else
            {
                ThemeManager.ChangeTheme(this, Steel, Theme.Dark);
                Properties.Settings.Default.DarkTheme = true;
                Properties.Settings.Default.Save();
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                ProfilePage ProfilePage = new ProfilePage();
                Client.SwitchPage(ProfilePage);
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                PlayPage PlayPage = new PlayPage();
                Client.SwitchPage(PlayPage);
            }
        }

        private void ShopButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                ShopPage ShopPage = new ShopPage();
                Client.SwitchPage(ShopPage);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            //if (Client.IsLoggedIn)
            //{
            SettingsPage SettingsPage = new SettingsPage();
            Client.SwitchPage(SettingsPage);
            //}
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                MainPage MainPage = new MainPage();
                Client.SwitchPage(MainPage);
            }
        }

        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}