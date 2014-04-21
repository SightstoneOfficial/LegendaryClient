using jabber.client;
using LegendaryClient.Logic;
using LegendaryClient.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using PVPNetConnect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
            Client.ExecutingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //Set up logging before we do anything
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "lcdebug.log")))
            {
                File.Delete(Path.Combine(Client.ExecutingDirectory, "lcdebug.log"));
            }

            Client.InfoLabel = InfoLabel;
            Client.PVPNet = new PVPNetConnection();
            Client.PVPNet.KeepDelegatesOnLogout = false;
            Client.PVPNet.OnError += Client.PVPNet_OnError;

            Steel = new Accent("Steel", new Uri("pack://application:,,,/LegendaryClient;component/Controls/Steel.xaml"));
            if (Properties.Settings.Default.DarkTheme)
            {
                ThemeManager.ChangeTheme(this, Steel, Theme.Dark);
            }

            Client.ChatClient = new JabberClient();
            ChatContainer.Content = new ChatPage().Content;
            StatusContainer.Content = new StatusPage().Content;
            NotificationOverlayContainer.Content = new FakePage().Content;
            NotificationContainer.Content = new NotificationsPage().Content;

            Grid NotificationTempGrid = null;
            foreach (var x in NotificationOverlayContainer.GetChildObjects())
            {
                if (x is Grid)
                {
                    NotificationTempGrid = x as Grid;
                }
            }

            Client.PlayButton = PlayButton;
            Client.Pages = new List<Page>();
            Client.MainGrid = MainGrid;
            Client.NotificationGrid = NotificationTempGrid;
            Client.MainWin = this;
            Client.Container = Container;
            Client.OverlayContainer = OverlayContainer;
            Client.ChatContainer = ChatContainer;
            Client.StatusContainer = StatusContainer;
            Client.NotificationContainer = NotificationContainer;
            Client.NotificationOverlayContainer = NotificationOverlayContainer;
            Client.SwitchPage(new PatcherPage());
        }

        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            //Disregard PVPNetSpam
            if (e.Exception.Message.Contains("too small for an Int32") || e.Exception.Message.Contains("Constructor on type "))
                return;
            Client.Log("A first chance exception was thrown", "EXCEPTION");
            Client.Log(e.Exception.Message, "EXCEPTION");
            Client.Log(e.Exception.StackTrace, "EXCEPTION");
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
            if (Client.IsLoggedIn)
            {
                SettingsPage SettingsPage = new SettingsPage();
                Client.SwitchPage(SettingsPage);
            }
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

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoLogin = false;
            if (Client.IsLoggedIn)
            {
                LoginPage page = new LoginPage();
                Client.Pages.Clear();
                Client.QuitCurrentGame();
                Client.PVPNet.Disconnect();
                Client.ChatClient.Close();
                Client.IsLoggedIn = false;
                Client.StatusContainer.Visibility = Visibility.Hidden;
                Client.Container.Margin = new Thickness(0, 0, 0, 0);
                Client.SwitchPage(page);
                Client.SwitchPage(new LoginPage());
            }
        }

    }
}