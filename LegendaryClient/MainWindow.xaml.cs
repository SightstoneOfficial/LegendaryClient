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
using System.Net;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace LegendaryClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Accent myAccent = null;
        Warning Warn = new Warning();
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();
            ReturnToPage.Visibility = Visibility.Hidden;
            //Client.ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //Keep this this way that way the auto updator knows what to update
            var ExecutingDirectory = System.IO.Directory.GetParent(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location));

            Client.ExecutingDirectory = ExecutingDirectory.ToString().Replace("file:\\", "");
            LCLog.WriteToLog.ExecutingDirectory = Client.ExecutingDirectory;
            LCLog.WriteToLog.LogfileName = "LegendaryClient.Log";
            LCLog.WriteToLog.CreateLogFile();
            AppDomain.CurrentDomain.FirstChanceException += LCLog.Log.CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += LCLog.Log.AppDomain_CurrentDomain;

            Client.InfoLabel = InfoLabel;
            Client.PVPNet = new PVPNetConnection();
            Client.PVPNet.KeepDelegatesOnLogout = false;
            Client.PVPNet.OnError += Client.PVPNet_OnError;
            if (String.IsNullOrEmpty(Properties.Settings.Default.Theme))
               Properties.Settings.Default.Theme = "pack://application:,,,/LegendaryClient;component/Controls/Steel.xaml";
            myAccent = new Accent("AccentName", new Uri(Properties.Settings.Default.Theme));
            ThemeManager.ChangeTheme(this, myAccent, (Properties.Settings.Default.DarkTheme) ? Theme.Dark : Theme.Light);

            Client.ChatClient = new JabberClient();
            Client.FriendList = new FriendList();
            ChatContainer.Content = Client.FriendList.Content;
            Client.notificationPage = new NotificationPage();
            NotificationContainer.Content = Client.notificationPage.Content;
            Client.statusPage = new StatusPage();
            StatusContainer.Content = Client.statusPage.Content;
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
            Client.BackgroundImage = BackImage;
            Client.NotificationGrid = NotificationTempGrid;
            Client.MainWin = this;
            Client.Container = Container;
            Client.OverlayContainer = OverlayContainer;
            Client.NotificationContainer = NotificationContainer;
            Client.ChatContainer = ChatContainer;
            Client.StatusContainer = StatusContainer;
            Client.ReturnButton = ReturnToPage;
            Client.NotificationOverlayContainer = NotificationOverlayContainer;
            Client.SoundPlayer = SoundPlayer;
            Client.AmbientSoundPlayer = ASoundPlayer;
            Client.SwitchPage(new PatcherPage());

            if (!String.IsNullOrEmpty(Properties.Settings.Default.LoginPageImage) && Properties.Settings.Default.UseAsBackgroundImage)
            {
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Properties.Settings.Default.LoginPageImage.Replace("\r\n", ""))))
                    Client.BackgroundImage.Source = new BitmapImage(new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Properties.Settings.Default.LoginPageImage), UriKind.Absolute));
            }
        }

        public void ChangeTheme()
        {
            Accent myAccent = new Accent("AccentName", new Uri(Properties.Settings.Default.Theme));
            ThemeManager.ChangeTheme(this, myAccent, (Properties.Settings.Default.DarkTheme) ? Theme.Dark : Theme.Light);
            Client.CurrentAccent = myAccent;
        }

        private void SwichToTeamQueue_Click(object Sender, RoutedEventArgs e)
        {
            Client.SwitchPage(Client.CurrentPage);
        }

        internal bool SwitchTeamPage = true;

        public new void Hide()
        {
            if (SwitchTeamPage == true)
            {
                ReturnToPage.Visibility = Visibility.Visible;
                SwitchTeamPage = false;
            }
            else if (SwitchTeamPage == false)
            {
                ReturnToPage.Visibility = Visibility.Hidden;
                SwitchTeamPage = true;
            }
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
                uiLogic.UpdateProfile(Client.LoginPacket.AllSummonerData.Summoner.Name);
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
                SettingsPage SettingsPage = new SettingsPage(this);
                Client.SwitchPage(SettingsPage);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                uiLogic.UpdateMainPage();
                Client.ClearPage(typeof(SettingsPage));
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
                Client.ReturnButton.Visibility = Visibility.Hidden;
                LoginPage page = new LoginPage();
                Client.Pages.Clear();
                Client.PVPNet.QuitGame();
                Client.PVPNet.Disconnect();
                Client.ChatClient.OnDisconnect -= Client.ChatClient_OnDisconnect;
                Client.ChatClient.Close();
                Client.ChatClient = null;
                Client.ChatClient = new JabberClient();
                Client.chatlistview.Children.Clear();
                Client.IsLoggedIn = false;
                Client.StatusContainer.Visibility = Visibility.Hidden;
                Client.Container.Margin = new Thickness(0, 0, 0, 0);
                Client.SwitchPage(new LoginPage());
            }
        }

        
        private bool QuitMe = false;
        
        private void MainWindow_Closing(Object sender, CancelEventArgs e)
        {
            Client.PVPNet.Leave();
            Client.PVPNet.PurgeFromQueues();
            Client.PVPNet.Disconnect();

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