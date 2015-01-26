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
using System.Windows.Media.Imaging;
using jabber.protocol.client;
using System.Threading;
using System.IO.Pipes;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using LegendaryClient.Properties;
using System.Security.Principal;
using System.Windows.Threading;
using System.Net;
using Microsoft.Win32;

namespace LegendaryClient
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Accent myAccent = null;
        Warning Warn = new Warning();
        public static bool started = false;
        private static readonly ILog log = log4net.LogManager.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            PresentationTraceSources.ResourceDictionarySource.Switch.Level = System.Diagnostics.SourceLevels.Critical;
            InitializeComponent();
            ReturnToPage.Visibility = Visibility.Hidden;
            //Client.ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //Keep this this way that way the auto updator knows what to update
            var ExecutingDirectory = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            Client.ExecutingDirectory = ExecutingDirectory.ToString().Replace("file:\\", "");
            var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("LegendaryClient");
            if (key != null)
            {
                key.SetValue("LCLocation", Client.ExecutingDirectory);
                key.Close();
            }
            LCLog.WriteToLog.ExecutingDirectory = Client.ExecutingDirectory;
            LCLog.WriteToLog.LogfileName = "LegendaryClient.Log";
            LCLog.WriteToLog.CreateLogFile();
            if (Client.Authenticate(Settings.Default.devKeyLoc, false))
            {
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "DevWin", "LCDevWindow.exe")))
                {
                    StartPipe();
                    AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                    AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                    Process.Start(Path.Combine(Client.ExecutingDirectory, "DevWin", "LCDevWindow.exe"));
                }
            }
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
            if (Properties.Settings.Default.incognitoLogin)
            {
                Client.FriendList.PresenceChanger.SelectedItem = "Invisible";
                Client.presenceStatus = "";
                Client.CurrentPresence = PresenceType.invisible;
            }
            else
            {
                Client.FriendList.PresenceChanger.SelectedItem = "Online";
                Client.presenceStatus = "chat";
                Client.CurrentPresence = PresenceType.available;
            }
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

            Client.FullNotificationOverlayContainer = FullNotificationOverlayContainer;
            Client.PlayButton = PlayButton;
            Client.Pages = new List<Page>();
            Client.MainGrid = MainGrid;
            Client.BackgroundImage = BackImage;
            Client.NotificationGrid = NotificationTempGrid;
            Client.MainWin = this;
            Client.Container = Container;
            Client.OverlayContainer = OverlayContainer;
            Client.OverOverlayContainer = OverOverlayContainer;
            Client.NotificationContainer = NotificationContainer;
            Client.ChatContainer = ChatContainer;
            Client.StatusContainer = StatusContainer;
            Client.ReturnButton = ReturnToPage;
            Client.inQueueTimer = inQueueTimer;
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

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Client.SendPIPE != null)
                Client.SendPIPE.WriteString("[" + "UnhandledException" + "] " + e.ExceptionObject);
        }

        void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (Client.SendPIPE != null)
                Client.SendPIPE.WriteString("[" + "Exception" + "] " + e.Exception.Message);
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
            {
                uiLogic.Profile.GetSummonerProfile(Client.LoginPacket.AllSummonerData.Summoner.Name);
                Client.SwitchPage(uiLogic.Profile);
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
            if (!Client.patching)
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
            else if (Client.donepatch)
            {
                Client.ClearPage(typeof(SettingsPage));
                Client.SwitchPage(new LoginPage());
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
            if (Client.IsLoggedIn && Client.GameStatus.ToLower() != "championSelect".ToLower())
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
                Client.ClearPage(typeof(MainPage));
            }
            else if (Properties.Settings.Default.warnClose && Client.IsInGame)
            {
                Warn = new Warning();
                Warn.Title.Content = "Logout while in Game";
                Warn.MessageText.Text = "Are You Sure You Want To Quit? This will result in a dodge.";
                Warn.backtochampselect.Click += HideWarning;
                Warn.AcceptButton.Click += Quit;
                Warn.hide.Click += HideWarning;
                Warn.backtochampselect.Content = "Return to Champ Select";
                Warn.AcceptButton.Content = "Dodge game and logout";
                Client.FullNotificationOverlayContainer.Content = Warn.Content;
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
            }
        }
        private void MainWindow_Closing(Object sender, CancelEventArgs e)
        {
            if (Properties.Settings.Default.warnClose || Client.curentlyRecording.Count > 0 || Client.IsInGame)
            {
                Warn = new Warning();
                e.Cancel = true;
                Warn.Title.Content = "Quit";
                Warn.MessageText.Text = "Are You Sure You Want To Quit?";
                Warn.backtochampselect.Click += HideWarning;
                Warn.AcceptButton.Click += Quit;
                Warn.hide.Click += HideWarning;
                if (Client.curentlyRecording.Count > 0)
                    Warn.MessageText.Text = "Game recorder is still running.\nIf you exit now then the replay won't be playable.\n" + Warn.MessageText.Text;
                Client.FullNotificationOverlayContainer.Content = Warn.Content;
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
            }
            else
            {
                if (Client.IsLoggedIn)
                {
                    Client.PVPNet.PurgeFromQueues();
                    Client.PVPNet.Leave();
                    Client.PVPNet.Disconnect();
                }
                Environment.Exit(0);
            }

        }
        private void Quit(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                Client.PVPNet.PurgeFromQueues();
                Client.PVPNet.Leave();
                Client.PVPNet.Disconnect();
            }
            Environment.Exit(0);
        }
        private void HideWarning(object sender, RoutedEventArgs e)
        {
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
        }

        private static int numThreads = 4;

        public void StartPipe()
        {
            int i = 0;
            Thread[] servers = new Thread[numThreads];
            if (i < numThreads)
                i++;
            servers[i] = new Thread(ServerThread);
            servers[i].Start();
        }
        private void ServerThread(object data)
        {
            NamedPipeServerStream pipeServer =
                new NamedPipeServerStream("LegendaryClientPipe@191537514598135486vneaoifjidafd", PipeDirection.InOut, numThreads);
            int threadId = Thread.CurrentThread.ManagedThreadId;
            pipeServer.WaitForConnection();
            Client.SendPIPE = new StreamString(pipeServer);
            Client.SendPIPE.WriteString("Logger started. All errors will be logged from now on");
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            Client.SendPIPE.WriteString("LegendaryClient Version: " + version);

            Client.SendPIPE.WriteString("AwaitStart");


            NamedPipeClientStream output = new NamedPipeClientStream(".", "LegendaryClientPipe@191537514598135486vneaoifjidafdOUTPUT", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            output.Connect();
            StreamString ss = new StreamString(output);
            Client.InPIPE = ss;
            started = true;
            while (started)
            {
                string x = ss.ReadString();
                if (x.Contains("SendOVERLAY"))
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        string[] mmm = x.Split('|');
                        var messageOver = new MessageOverlay { MessageTitle = { Content = mmm[1] }, MessageTextBox = { Text = mmm[2] } };
                        if (!x.ToLower().Contains("fullover"))
                        {
                            Client.OverlayContainer.Content = messageOver.Content;
                            Client.OverlayContainer.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            Client.FullNotificationOverlayContainer.Content = messageOver.Content;
                            Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
                        }
                        Client.SendPIPE.WriteString("Overlay received!");
                    }));
                }
                else if (x == "Server_STOPPED")
                {
                    started = false;
                }
            }
        }
    }

    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;
            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            if (len > 0)
            {
                byte[] inBuffer = new byte[len];
                ioStream.Read(inBuffer, 0, len);

                return streamEncoding.GetString(inBuffer);
            }
            else
                return string.Empty;

        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}