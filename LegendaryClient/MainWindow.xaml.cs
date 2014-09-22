using jabber.client;
using LegendaryClient.Logic;
using LegendaryClient.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using PVPNetConnect;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using log4net;
using log4net.Config;
using System.Security.Permissions;

namespace LegendaryClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Accent Steel = null;
        Warning Warn = new Warning();
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();

            SwitchPage.Visibility = Visibility.Hidden;
            Client.ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                        
            LCLog.WriteToLog.ExecutingDirectory = Client.ExecutingDirectory;
            LCLog.WriteToLog.LogfileName = "LegendaryClient.Log";
            LCLog.WriteToLog.CreateLogFile();
            AppDomain.CurrentDomain.FirstChanceException += LCLog.Log.CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += LCLog.Log.AppDomain_CurrentDomain;
#if !DEBUG

#endif

            Client.InfoLabel = InfoLabel;
            Client.StartHeartbeat();
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
            Client.LobbyButton = SwitchPage;
            Client.NotificationOverlayContainer = NotificationOverlayContainer;
            Client.SoundPlayer = SoundPlayer;
            Client.AmbientSoundPlayer = ASoundPlayer;
            Client.SwitchPage(new PatcherPage());

            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainWindow_Closing);
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

        private void SwichToTeamQueue_Click(object Sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new TeamQueuePage(null, null, true));
        }
        internal bool SwitchTeamPage = true;
        public void Hide()
        {
            if (SwitchTeamPage == true)
            {
                SwitchPage.Visibility = Visibility.Visible;
                SwitchTeamPage = false;
            }
            else if (SwitchTeamPage == false)
            {
                SwitchPage.Visibility = Visibility.Hidden;
                SwitchTeamPage = true;
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
            if (Client.IsLoggedIn)
            {
                ReplayPage ReplayPage = new ReplayPage();
                Client.SwitchPage(ReplayPage);
            }
        }

        #pragma warning disable 4014 //Code does not need to be awaited
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.AutoLogin = false;
            if (Client.IsLoggedIn)
            {
                LoginPage page = new LoginPage();
                Client.Pages.Clear();
                Client.PVPNet.QuitGame();
                Client.PVPNet.Disconnect();
                Client.ChatClient.Close();
                Client.IsLoggedIn = false;
                Client.StatusContainer.Visibility = Visibility.Hidden;
                Client.Container.Margin = new Thickness(0, 0, 0, 0);
                Client.SwitchPage(new LoginPage());
            }
        }

        
        private bool QuitMe = false;
        
        private void MainWindow_Closing(Object sender, CancelEventArgs e)
        {
            Client.PVPNet.PurgeFromQueues();
            Client.PVPNet.Leave();
            Environment.Exit(0);

            if (QuitMe == true)
            {
                e.Cancel = true;
                Warn.Title.Content = "Quit";
                Warn.Content.Content = "Are You Sure You Want To Quit?";
                Warn.backtochampselect.Click += Quit;
                Warn.backtochampselect.Content = "Quit";
                Warn.AcceptButton.Click += HideWarning;
                Warn.hide.Click += HideWarning;
                Client.OverlayContainer.Content = new Warning().Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
            }
            else
            {
                e.Cancel = false;
            }
            
        }
        private void Quit(object sender, RoutedEventArgs e)
        {
            Client.PVPNet.PurgeFromQueues();
            Client.PVPNet.Leave();
            Environment.Exit(0);
        }
        private void HideWarning(object sender, RoutedEventArgs e)
        {
            Warn.Visibility = Visibility.Hidden;
        }
    }
}