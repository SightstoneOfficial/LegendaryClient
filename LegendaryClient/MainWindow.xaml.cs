using LegendaryClient.Logic;
using LegendaryClient.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading;
using System.IO.Pipes;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LegendaryClient.Properties;
using System.Windows.Threading;
using System.Net;
using Microsoft.Win32;
using LegendaryClient.Logic.Riot;
using agsXMPP.protocol.client;
using System.Security.Principal;

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

        public MainWindow(StartupEventArgs e)
        {
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
            PresentationTraceSources.ResourceDictionarySource.Switch.Level = SourceLevels.Critical;
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
            if (Directory.Exists(Path.Combine(Client.ExecutingDirectory, "GarenaClient")))
            {
                var regKey = Registry.CurrentUser.CreateSubKey("LegendaryClient");
                var val = regKey.GetValue("GarenaLocation").ToString();
                var garenaLocation = Path.Combine(Path.GetDirectoryName(val), "Air");
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "GarenaClient", "LolClient.exe.real")))
                {
                    if (File.Exists(Path.Combine(garenaLocation, "LolClient.exe")))
                        File.Delete(Path.Combine(garenaLocation, "LolClient.exe"));
                    File.Move(Path.Combine(Client.ExecutingDirectory, "GarenaClient", "LolClient.exe.real"), Path.Combine(garenaLocation, "LolClient.exe"));
                }
                Directory.Delete(Path.Combine(Client.ExecutingDirectory, "GarenaClient"));
            }

            LCLog.WriteToLog.ExecutingDirectory = Client.ExecutingDirectory;
            LCLog.WriteToLog.LogfileName = "LegendaryClient.Log";
            LCLog.WriteToLog.CreateLogFile();
#if DEBUG
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "DevWin", "LCDevWindow.exe")))
            {
                StartPipe();
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                Process.Start(Path.Combine(Client.ExecutingDirectory, "DevWin", "LCDevWindow.exe"));
            }
#endif
            AppDomain.CurrentDomain.FirstChanceException += LCLog.Log.CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += LCLog.Log.AppDomain_CurrentDomain;

            Client.UserTitleBarLabel = UserTitleBarLabel;
            Client.UserTitleBarImage = UserTitleBarImage;
            Client.InfoLabel = InfoLabel;
            //RiotCalls = new PVPNetConnection { KeepDelegatesOnLogout = false };
            //RiotCalls.OnError += RiotCalls_OnError;
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }
            if (string.IsNullOrEmpty(Settings.Default.Theme))
                Properties.Settings.Default.Theme = "pack://application:,,,/LegendaryClient;component/Controls/Steel.xaml";
            myAccent = new Accent("AccentName", new System.Uri(Settings.Default.Theme));
            ThemeManager.ChangeAppStyle(this, myAccent, (Settings.Default.DarkTheme) ? ThemeManager.GetAppTheme("BaseDark") : ThemeManager.GetAppTheme("BaseLight"));//,

            Client.FriendList = new FriendList();
            if (Properties.Settings.Default.incognitoLogin)
            {
                Client.FriendList.PresenceChanger.SelectedItem = "Invisible";
                Client.presenceStatus = ShowType.NONE;
                Client.CurrentPresence = PresenceType.invisible;
            }
            else
            {
                Client.FriendList.PresenceChanger.SelectedItem = "Online";
                Client.presenceStatus = ShowType.chat;
                Client.CurrentPresence = PresenceType.available;
            }
            ChatContainer.Content = Client.FriendList.Content;
            Client.notificationPage = new NotificationPage();
            NotificationContainer.Content = Client.notificationPage.Content;
            Client.statusPage = new StatusPage();
            StatusContainer.Content = Client.statusPage.Content;
            //NotificationOverlayContainer.Content = new FakePage().Content;
            NotificationOverlayContainer.Content = new Grid();
            Grid NotificationTempGrid = null;
            foreach (var x in NotificationOverlayContainer.GetChildObjects().OfType<Grid>())
                NotificationTempGrid = x;

            ChangeTheme();

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

            if (!string.IsNullOrEmpty(Settings.Default.LoginPageImage) && Properties.Settings.Default.UseAsBackgroundImage)
            {
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Properties.Settings.Default.LoginPageImage.Replace("\r\n", ""))))
                    Client.BackgroundImage.Source = new BitmapImage(new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Properties.Settings.Default.LoginPageImage), UriKind.Absolute));
            }
            // screen resolution fix: because MainWindow height is bigger than some screen height
            if(SystemParameters.WorkArea.Height < this.Height)
            {
                // resize LC to MinHeight -- a UI designer may edit this
                this.SizeToContent = SizeToContent.Manual;
                this.Width = this.MinWidth;
                this.Height = this.MinHeight;
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
            Accent myAccent = new Accent("AccentName", new System.Uri(Settings.Default.Theme));
            ThemeManager.ChangeAppStyle(Application.Current, myAccent, (Settings.Default.DarkTheme) ? ThemeManager.GetAppTheme("BaseDark") : ThemeManager.GetAppTheme("BaseLight"));
            var random = new Random().Next(-9000, 9000);
            myAccent = new Accent(random.ToString(), new System.Uri(Settings.Default.Theme));
            ThemeManager.ChangeAppStyle(Application.Current, myAccent, (Settings.Default.DarkTheme) ? ThemeManager.GetAppTheme("BaseDark") : ThemeManager.GetAppTheme("BaseLight"));
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
                Client.SwitchPage(Client.Profile);
            }       
        }

        private void ProfileButton_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                Client.Profile.GetSummonerProfile(Client.LoginPacket.AllSummonerData.Summoner.Name);
                Client.SwitchPage(Client.Profile);
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
                SettingsPage SettingsPage = new SettingsPage();
                Client.SwitchPage(SettingsPage);
            }
        }

        private void PluginsButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                PluginsPage PluginsPage = new PluginsPage();
                Client.SwitchPage(PluginsPage);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                Client.SwitchPage(Client.MainPage);
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
        public void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.AutoLogin = false;
            if (Client.IsLoggedIn && !string.Equals(Client.GameStatus, "championSelect", StringComparison.CurrentCultureIgnoreCase))
            {
                Client.ReturnButton.Visibility = Visibility.Hidden;
                (Client.MainWin as MainWindow).FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
                LoginPage page = new LoginPage();
                Client.Pages.Clear();
                RiotCalls.QuitGame();
                Client.RiotConnection.Disconnected -= Client.RiotConnection_Disconnected;
                Client.RiotConnection.Close();
                Client.XmppConnection.OnError -= Client.XmppConnection_OnError;
                Client.XmppConnection.Close();
                Client.XmppConnection = null;
                Client.chatlistview.Children.Clear();
                Client.IsLoggedIn = false;
                Client.StatusContainer.Visibility = Visibility.Hidden;
                Client.Container.Margin = new Thickness(0, 0, 0, 0);
                Client.SwitchPage(new LoginPage());
                Client.ClearPage(typeof(MainPage));
            }
            else if (Settings.Default.warnClose && Client.IsInGame)
            {
                Warn = new Warning
                {
                    Header = { Content = "Logout while in Game" },
                    MessageText = { Text = "Are You Sure You Want To Quit? This will result in a dodge." }
                };
                Warn.backtochampselect.Click += HideWarning;
                Warn.AcceptButton.Click += Quit;
                Warn.hide.Click += HideWarning;
                Warn.backtochampselect.Content = "Return to Champ Select";
                Warn.AcceptButton.Content = "Dodge game and logout";
                Client.FullNotificationOverlayContainer.Content = Warn.Content;
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
            }
        }
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (Properties.Settings.Default.warnClose || Client.curentlyRecording.Count > 0 || Client.IsInGame)
            {
                Warn = new Warning();
                e.Cancel = true;
                Warn.Header.Content = "Quit";
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
                    RiotCalls.PurgeFromQueues();
                    RiotCalls.Leave();
                    Client.RiotConnection.Close();
                }
                Environment.Exit(0);
            }

        }
        private void Quit(object sender, RoutedEventArgs e)
        {
            if (Client.IsLoggedIn)
            {
                RiotCalls.PurgeFromQueues();
                RiotCalls.Leave();
                Client.RiotConnection.Close();
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
                new NamedPipeServerStream("LegendaryClientPipe@191537514598135486vneaoifJidafd", PipeDirection.InOut, numThreads);
            int threadId = Thread.CurrentThread.ManagedThreadId;
            pipeServer.WaitForConnection();
            Client.SendPIPE = new StreamString(pipeServer);
            Client.SendPIPE.WriteString("Logger started. All errors will be logged from now on");
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            Client.SendPIPE.WriteString("LegendaryClient Version: " + version);

            Client.SendPIPE.WriteString("AwaitStart");


            NamedPipeClientStream output = new NamedPipeClientStream(".", "LegendaryClientPipe@191537514598135486vneaoifJidafdOUTPUT", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
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
            if (len > ushort.MaxValue)
            {
                len = (int)ushort.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}