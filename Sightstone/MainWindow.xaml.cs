using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using agsXMPP;
using agsXMPP.protocol.client;
using LCLog;
using Sightstone.Logic;
using Sightstone.Logic.Riot;
using Sightstone.Properties;
using Sightstone.Windows;
using MahApps.Metro;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using Uri = System.Uri;
using Sightstone.Logic.MultiUser;

namespace MaterialDesignThemes.Wpf
{
    /// <summary>
    /// A card is a content control, styled according to Material Design guidelines.
    /// </summary>
    public class Card : ContentControl
    {
        static Card()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Card), new FrameworkPropertyMetadata(typeof(Card)));
        }
    }
}

namespace Sightstone
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static bool Started;
        private Accent _myAccent;
        private Warning _warn = new Warning();
        internal bool SwitchTeamPage = true;

        public MainWindow()
        {
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
            PresentationTraceSources.ResourceDictionarySource.Switch.Level = SourceLevels.Critical;
            InitializeComponent();
            InitializeLanguage();
            Initialize();
            InitializeGui();
        }

        /// <summary>
        /// Adds the language file
        /// </summary>
        private static void InitializeLanguage()
        {
            var lid = Thread.CurrentThread.CurrentCulture.ToString().Contains("-")
                    ? Thread.CurrentThread.CurrentCulture.ToString().Split('-')[0].ToUpperInvariant()
                    : Thread.CurrentThread.CurrentCulture.ToString().ToUpperInvariant();

            switch (lid)
            {
                case "EN":
                    Client.Dict.Source = new Uri("..\\Logic\\Languages\\English.xaml",
                                  UriKind.Relative);
                    break;
                default:
                    Client.Dict.Source = new Uri("..\\Logic\\Languages\\English.xaml",
                                      UriKind.Relative);
                    break;
            }
            Application.Current.Resources.MergedDictionaries.Add(Client.Dict);
        }

        private void InitializeGui()
        {
            ReturnToPage.Visibility = Visibility.Hidden;
            Client.UserTitleBarLabel = UserTitleBarLabel;
            Client.UserTitleBarImage = UserTitleBarImage;
            Client.InfoLabel = InfoLabel;


            if (string.IsNullOrEmpty(Settings.Default.Theme))
                Settings.Default.Theme = "pack://application:,,,/Sightstone;component/Controls/Steel.xaml";
            _myAccent = new Accent("AccentName", new Uri(Settings.Default.Theme));
            ThemeManager.ChangeAppStyle(this,
                _myAccent,
                (Settings.Default.DarkTheme)
                    ? ThemeManager.GetAppTheme("BaseDark")
                    : ThemeManager.GetAppTheme("BaseLight"));

            ChatContainer.Content = Client.FriendList.Content;

            Client.notificationPage = new NotificationPage();
            NotificationContainer.Content = Client.notificationPage.Content;

            Client.statusPage = new StatusPage();
            StatusContainer.Content = Client.statusPage.Content;

            NotificationOverlayContainer.Content = new Grid();
            Grid notificationTempGrid = null;
            foreach (var x in NotificationOverlayContainer.GetChildObjects().OfType<Grid>())
                notificationTempGrid = x;

            ChangeTheme();

            Client.FullNotificationOverlayContainer = FullNotificationOverlayContainer;
            Client.PlayButton = PlayButton;
            Client.Pages = new List<Page>();
            Client.MainGrid = MainGrid;
            Client.BackgroundImage = BackImage;
            Client.NotificationGrid = notificationTempGrid;
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
            Client.SwitchPage(new LoginPage());

            if (!string.IsNullOrEmpty(Settings.Default.LoginPageImage) && Settings.Default.UseAsBackgroundImage)
            {
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Settings.Default.LoginPageImage.Replace("\r\n", ""))))
                    Client.BackgroundImage.Source =
                        new BitmapImage(new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", Settings.Default.LoginPageImage), UriKind.Absolute));
            }

            // screen resolution fix: because MainWindow height is bigger than some screen height
            if (SystemParameters.WorkArea.Height < Height)
            {
                // resize LC to MinHeight -- a UI designer may edit this
                SizeToContent = SizeToContent.Manual;
                Width = MinWidth;
                Height = MinHeight;
            }
        }

        private void Initialize()
        {
            //Settings
            if (Settings.Default.UpgradeRequired)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpgradeRequired = false;
                Settings.Default.Save();
            }

            //Chat connection
            Client.FriendList = new FriendList {PresenceChanger = {SelectedItem = Settings.Default.incognitoLogin
                ? "Invisible"
                : "Online"}};

            //Client.ExecutingDirectory = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            //Keep this this way that way the auto updator knows what to update

            // ReSharper disable once AssignNullToNotNullAttribute
            var executingDirectory = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            Client.ExecutingDirectory = executingDirectory.FullName.Replace("file:\\", "");

            //Log
            WriteToLog.ExecutingDirectory = Client.ExecutingDirectory;
            WriteToLog.LogfileName = "Sightstone.Log";
            WriteToLog.CreateLogFile();

            //Registry
            var key = Registry.CurrentUser.CreateSubKey("Sightstone");
            if (key != null)
            {
                key.SetValue("LCLocation", Client.ExecutingDirectory);
                key.Close();
            }

            //Garena
            if (Directory.Exists(Path.Combine(Client.ExecutingDirectory, "GarenaClient")))
            {
                var regKey = Registry.CurrentUser.CreateSubKey("Sightstone");
                // ReSharper disable once PossibleNullReferenceException
                var val = regKey.GetValue("GarenaLocation").ToString();
                // ReSharper disable once AssignNullToNotNullAttribute
                var garenaLocation = Path.Combine(Path.GetDirectoryName(val), "Air");
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "GarenaClient", "LolClient.exe.real")))
                {
                    if (File.Exists(Path.Combine(garenaLocation, "LolClient.exe")))
                        File.Delete(Path.Combine(garenaLocation, "LolClient.exe"));
                    File.Move(Path.Combine(Client.ExecutingDirectory, "GarenaClient", "LolClient.exe.real"), Path.Combine(garenaLocation, "LolClient.exe"));
                }
                Directory.Delete(Path.Combine(Client.ExecutingDirectory, "GarenaClient"));
            }

#if DEBUG
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "DevWin", "LCDevWindow.exe")))
            {
                StartPipe();
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
                Process.Start(Path.Combine(Client.ExecutingDirectory, "DevWin", "LCDevWindow.exe"));
            }
#endif
            AppDomain.CurrentDomain.FirstChanceException += Log.CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.UnhandledException += Log.AppDomain_CurrentDomain;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (Client.SendPIPE != null)
                Client.SendPIPE.WriteString("[" + "UnhandledException" + "] " + e.ExceptionObject);
        }

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            if (Client.SendPIPE != null)
                Client.SendPIPE.WriteString("[" + "Exception" + "] " + e.Exception.Message);
        }

        public void ChangeTheme()
        {
            var myAccent = new Accent("AccentName", new Uri(Settings.Default.Theme));
            ThemeManager.ChangeAppStyle(Application.Current,
                myAccent,
                (Settings.Default.DarkTheme)
                    ? ThemeManager.GetAppTheme("BaseDark")
                    : ThemeManager.GetAppTheme("BaseLight"));
            var random = new Random().Next(-9000, 9000);
            myAccent = new Accent(random.ToString(), new Uri(Settings.Default.Theme));
            ThemeManager.ChangeAppStyle(Application.Current,
                myAccent,
                (Settings.Default.DarkTheme)
                    ? ThemeManager.GetAppTheme("BaseDark")
                    : ThemeManager.GetAppTheme("BaseLight"));
            Client.CurrentAccent = myAccent;
        }

        private void SwichToTeamQueue_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(Client.CurrentPage);
        }

        public new void Hide()
        {
            if (SwitchTeamPage)
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
            if (UserList.Users.Count > 0 && !String.IsNullOrWhiteSpace(Client.CurrentUser))
            {
                Client.SwitchPage(Client.Profile);
            }
        }

        private void ProfileButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UserList.Users.Count > 0 && !string.IsNullOrWhiteSpace(Client.CurrentUser))
            {
                UserClient UserClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];
                Client.Profile.GetSummonerProfile(UserClient.LoginPacket.AllSummonerData.Summoner.Name);
                Client.SwitchPage(Client.Profile);
            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.Users.Count > 0 && !String.IsNullOrWhiteSpace(Client.CurrentUser))
            {
                var playPage = new PlayPage();
                Client.SwitchPage(playPage);
            }
        }

        private void ShopButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.Users.Count > 0 && !String.IsNullOrWhiteSpace(Client.CurrentUser))
            {
                var shopPage = new ShopPage();
                Client.SwitchPage(shopPage);
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Client.patching)
            {
                var settingsPage = new SettingsPage();
                Client.SwitchPage(settingsPage);
            }
        }

        private void PluginsButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.Users.Count > 0 && !String.IsNullOrWhiteSpace(Client.CurrentUser))
            {
                var pluginsPage = new PluginsPage();
                Client.SwitchPage(pluginsPage);
            }
        }

        private void ScreenshotsButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.Users.Count > 0 && !String.IsNullOrWhiteSpace(Client.CurrentUser))
            {
                var screenshotsPage = new ScreenshotsPage();
                Client.SwitchPage(screenshotsPage);
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.Users.Count > 0 && !String.IsNullOrWhiteSpace(Client.CurrentUser))
            {
                Client.SwitchPage(Client.MainPage);
                Client.ClearPage(typeof (SettingsPage));
            }
            else if (Client.donepatch)
            {
                Client.ClearPage(typeof (SettingsPage));
                Client.SwitchPage(new LoginPage());
            }
        }

        private void ReplayButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserList.Users.Count > 0 && !String.IsNullOrWhiteSpace(Client.CurrentUser))
            {
                var replayPage = new ReplayPage();
                Client.SwitchPage(replayPage);
            }
        }

        public void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.AutoLogin = false;
            if (UserList.Users.Count > 0 && !String.IsNullOrWhiteSpace(Client.CurrentUser))
                return;
            if (Client.CurrentUser == string.Empty)
                return;
            if (string.Equals((UserList.Users[Client.CurrentServer])[Client.CurrentUser].GameStatus, "championSelect", StringComparison.CurrentCultureIgnoreCase))
            {
                Client.ReturnButton.Visibility = Visibility.Hidden;
                // ReSharper disable once PossibleNullReferenceException
                (Client.MainWin as MainWindow).FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
                Client.Pages.Clear();
                (UserList.Users[Client.CurrentServer])[Client.CurrentUser].calls.QuitGame();
                Client.StatusContainer.Visibility = Visibility.Hidden;
                Client.Container.Margin = new Thickness(0, 0, 0, 0);
                Client.SwitchPage(new LoginPage());
                Client.ClearPage(typeof (MainPage));
            }
            else if (Settings.Default.warnClose && (UserList.Users[Client.CurrentServer])[Client.CurrentUser].IsInGame)
            {
                _warn = new Warning
                {
                    Header = { Content = Client.GetDictText("LogoutWhileInGame") },
                    MessageText = { Text = Client.GetDictText("LogoutWhileInGameNote") }
                };
                _warn.backtochampselect.Click += HideWarning;
                _warn.AcceptButton.Click += Quit;
                _warn.hide.Click += HideWarning;
                _warn.backtochampselect.Content = Client.GetDictText("ReturnToChampSelect");
                _warn.AcceptButton.Content = Client.GetDictText("DodgeGameAndLogout");
                Client.FullNotificationOverlayContainer.Content = _warn.Content;
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
            }
            else
            {
                Client.ReturnButton.Visibility = Visibility.Hidden;
                // ReSharper disable once PossibleNullReferenceException
                (Client.MainWin as MainWindow).FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
                Client.Pages.Clear();
                Client.StatusContainer.Visibility = Visibility.Hidden;
                Client.Container.Margin = new Thickness(0, 0, 0, 0);
                Client.SwitchPage(new LoginPage());
                Client.ClearPage(typeof(MainPage));
            }
        }

#pragma warning disable 4014

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (UserList.Users.Count > 0 && String.IsNullOrWhiteSpace(Client.CurrentUser) && Settings.Default.warnClose)
            {
                _warn = new Warning();
                e.Cancel = true;
                _warn.Header.Content = Client.GetDictText("Quit");
                _warn.MessageText.Text = Client.GetDictText("QuitNote");
                _warn.backtochampselect.Click += HideWarning;
                _warn.AcceptButton.Click += Quit;
                _warn.hide.Click += HideWarning;
                Client.FullNotificationOverlayContainer.Content = _warn.Content;
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
            }
            else if (Settings.Default.warnClose || (UserList.Users[Client.CurrentServer])[Client.CurrentUser].curentlyRecording.Count > 0 || (UserList.Users[Client.CurrentServer])[Client.CurrentUser].IsInGame)
            {
                _warn = new Warning();
                e.Cancel = true;
                _warn.Header.Content = Client.GetDictText("Quit");
                _warn.MessageText.Text = Client.GetDictText("QuitNote");
                _warn.backtochampselect.Click += HideWarning;
                _warn.AcceptButton.Click += Quit;
                _warn.hide.Click += HideWarning;
                if (UserList.Users != null && UserList.Users.ContainsKey(Client.CurrentUser))
                {
                    if ((UserList.Users[Client.CurrentServer])[Client.CurrentUser].curentlyRecording.Count > 0)
                        _warn.MessageText.Text = Client.GetDictText("GameRecorderRunning") + _warn.MessageText.Text;
                }
                Client.FullNotificationOverlayContainer.Content = _warn.Content;
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
            }
            else
            {
                Quit(null, null);
                Environment.Exit(0);
            }
        }

        private void Quit(object sender, RoutedEventArgs e)
        {
            foreach (var userclients in UserList.Users.SelectMany(user => user.Value.Where(userclients => userclients.Value.IsLoggedIn))) 
            {
                userclients.Value.calls.PurgeFromQueues();
                userclients.Value.calls.Leave();
                userclients.Value.RiotConnection.Close();
            }
            Environment.Exit(0);
        }

        private void HideWarning(object sender, RoutedEventArgs e)
        {
            Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;
        }

        private const int NumThreads = 4;
        public void StartPipe()
        {
            var i = 0;
            var servers = new Thread[NumThreads];
            if (i < NumThreads)
                i++;
            servers[i] = new Thread(ServerThread);
            servers[i].Start();
        }

        private void ServerThread(object data)
        {
            var pipeServer = new NamedPipeServerStream("SightstonePipe@191537514598135486vneaoifJidafd", PipeDirection.InOut, NumThreads);
            pipeServer.WaitForConnection();
            Client.SendPIPE = new StreamString(pipeServer);
            Client.SendPIPE.WriteString(Client.GetDictText("DevWinLoggerStarted"));
            var assembly = Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            var version = fvi.FileVersion;
            Client.SendPIPE.WriteString(Client.GetDictText("DevWinSighstoneVersion") + version);

            Client.SendPIPE.WriteString("AwaitStart");


            var output = new NamedPipeClientStream(".",
                "SightstonePipe@191537514598135486vneaoifJidafdOUTPUT",
                PipeDirection.InOut,
                PipeOptions.None,
                TokenImpersonationLevel.Impersonation);
            output.Connect();
            var ss = new StreamString(output);
            Client.InPIPE = ss;
            Started = true;
            while (Started)
            {
                var x = ss.ReadString();
                if (x.Contains("SendOVERLAY"))
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input,
                        new ThreadStart(() =>
                        {
                            var mmm = x.Split('|');
                            var messageOver = new MessageOverlay {MessageTitle = {Content = mmm[1]}, MessageTextBox = {Text = mmm[2]}};
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
                            Client.SendPIPE.WriteString(Client.GetDictText("DevWinOverlayReceived"));
                        }));
                }
                else if (x == "Server_STOPPED")
                {
                    Started = false;
                }
            }
        }

        private void TrayIcon_FocusClient(object sender, RoutedEventArgs e)
        {
            Client.FocusClient();
        }

        private void ShowHideClientMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                Client.FocusClient();
                ShowHideClientMenuItem.Header = "Hide";
            }
            else
            {
                this.WindowState = WindowState.Minimized;
                ShowHideClientMenuItem.Header = "Show";
            }
        }

        private void LegendaryWindow_StateChanged(object sender, EventArgs e)
        {
            ShowHideClientMenuItem.Header = this.WindowState == WindowState.Minimized ? "Show" : "Hide";
        }
    }

    public class StreamString
    {
        private readonly Stream _ioStream;
        private readonly UnicodeEncoding _streamEncoding;

        public StreamString(Stream ioStream)
        {
            _ioStream = ioStream;
            _streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            var len = _ioStream.ReadByte()*256;
            len += _ioStream.ReadByte();
            if (len > 0)
            {
                var inBuffer = new byte[len];
                _ioStream.Read(inBuffer, 0, len);

                return _streamEncoding.GetString(inBuffer);
            }
            return string.Empty;
        }

        public int WriteString(string outString)
        {
            var outBuffer = _streamEncoding.GetBytes(outString);
            var len = outBuffer.Length;
            if (len > ushort.MaxValue)
            {
                len = ushort.MaxValue;
            }
            _ioStream.WriteByte((byte) (len/256));
            _ioStream.WriteByte((byte) (len & 255));
            _ioStream.Write(outBuffer, 0, len);
            _ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}