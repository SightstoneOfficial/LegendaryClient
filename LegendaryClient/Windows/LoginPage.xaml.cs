using LegendaryClient.Logic;
using LegendaryClient.Logic.JSON;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Logic.SWF;
using LegendaryClient.Logic.SWF.SWFTypes;
using LegendaryClient.Logic.UpdateRegion;
using LegendaryClient.Properties;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using LegendaryClient.Controls;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.Riot.Platform;
using RtmpSharp.IO;
using RtmpSharp.Messaging;
using RtmpSharp.Net;
using Brush = System.Windows.Media.Brush;
using Point = System.Windows.Point;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.client;
using System.Security.Principal;
using System.Security.Cryptography;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage
    {
        private bool shouldExit = false;

        public LoginPage()
        {
            InitializeComponent();
            Client.donepatch = true;
            Client.patching = false;
            Version.TextChanged += WaterTextbox_TextChanged;
            bool x = Settings.Default.DarkTheme;
            if (!x)
            {
                var bc = new BrushConverter();
                HideGrid.Background = (Brush)bc.ConvertFrom("#B24F4F4F");
                LoggingInProgressRing.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
            }
            //#B2C8C8C8
            UpdateRegionComboBox.SelectedValue = Client.UpdateRegion;
            switch (Client.UpdateRegion)
            {
                case "PBE":
                    RegionComboBox.ItemsSource = new[] { "PBE" };
                    LoginUsernameBox.Visibility = Visibility.Visible;
                    RememberUsernameCheckbox.Visibility = Visibility.Visible;
                    break;

                case "Live":
                    RegionComboBox.ItemsSource = new[] { "BR", "EUNE", "EUW", "NA", "OCE", "RU", "LAS", "LAN", "TR", "CS" };
                    LoginUsernameBox.Visibility = Visibility.Visible;
                    RememberUsernameCheckbox.Visibility = Visibility.Visible;
                    break;

                case "Korea":
                    RegionComboBox.ItemsSource = new[] { "KR" };
                    LoginUsernameBox.Visibility = Visibility.Visible;
                    RememberUsernameCheckbox.Visibility = Visibility.Visible;
                    LoginPasswordBox.Visibility = Visibility.Visible;
                    break;

                case "Garena":
                    RegionComboBox.ItemsSource = new[] { "PH", "SG", "SGMY", "TH", "TW", "VN", "ID" };
                    LoginUsernameBox.Visibility = Visibility.Hidden;
                    RememberUsernameCheckbox.Visibility = Visibility.Hidden;
                    LoginPasswordBox.Visibility = Visibility.Hidden;
                    if (!string.IsNullOrEmpty(Settings.Default.DefaultGarenaRegion))
                        RegionComboBox.SelectedValue = Settings.Default.DefaultGarenaRegion;  // Default Garena Region
                    break;
            }

            string themeLocation = Path.Combine(Client.ExecutingDirectory, "assets", "themes", Client.Theme);
            if (!Settings.Default.DisableLoginMusic)
            {
                string[] music;
                string soundpath = Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "sound_o_heaven.ogg");
                if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
                {
                    if (!File.Exists(soundpath))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            wc.DownloadFile("http://images.wikia.com/leagueoflegends/images/1/10/Teemo.laugh3.ogg", soundpath);
                        }
                    }
                    music = new[] { soundpath };
                }
                else
                {
                    music = Directory.GetFiles(themeLocation, "*.mp3");
                }
                SoundPlayer.Source = new System.Uri(Path.Combine(themeLocation, music[0]));
                SoundPlayer.Play();
                Sound.IsChecked = false;
            }
            else Sound.IsChecked = true;

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
            {
                string SkinPath = Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                    "Teemo_Splash_" + new Random().Next(0, 8) + ".jpg");
                if (File.Exists(SkinPath))
                {
                    LoginImage.Source = new BitmapImage(new System.Uri(SkinPath, UriKind.Absolute));
                }
            }
            else if (Settings.Default.LoginPageImage == "")
            {
                string[] videos = Directory.GetFiles(themeLocation, "*.mp4");
                if (videos.Length > 0 && File.Exists(videos[0]))
                    LoginPic.Source = new System.Uri(videos[0]);
                LoginPic.LoadedBehavior = MediaState.Manual;
                LoginPic.MediaEnded += LoginPic_MediaEnded;
                SoundPlayer.MediaEnded += SoundPlayer_MediaEnded;
                LoginPic.Play();
            }
            else
            {
                if (
                    File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                        Settings.Default.LoginPageImage.Replace("\r\n", ""))))
                    LoginImage.Source =
                        new BitmapImage(
                            new System.Uri(
                                Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                                    Settings.Default.LoginPageImage), UriKind.Absolute));
            }

            Video.IsChecked = false;

            //Get client data after patcher completed

            Client.SQLiteDatabase = new SQLiteConnection(Path.Combine(Client.ExecutingDirectory, Client.sqlite));

            // Check database error
            try
            {
                Client.Champions = (from s in Client.SQLiteDatabase.Table<champions>()
                                    orderby s.name
                                    select s).ToList();

                //* to remove just remove one slash from the comment
                //Pls bard the tard for april fools
                Client.Champions.Find(bard => bard.name == "Bard").displayName = "Tard";
                //*/
            }
            catch (Exception e) // Database broken?
            {
                Client.Log("Database is broken : \r\n" + e.Message + "\r\n" + e.Source);
                var overlay = new MessageOverlay
                {
                    MessageTextBox = { Text = "Database is broken. Click OK to exit LegendaryClient." },
                    MessageTitle = { Content = "Database Error" }
                };
                Client.SQLiteDatabase.Close();
                File.Delete(Path.Combine(Client.ExecutingDirectory, Client.sqlite));
                overlay.AcceptButton.Click += (o, i) => { Environment.Exit(0); };
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
                return;
            }

            var FreeToPlay = FreeToPlayChampions.GetInstance();

            foreach (champions c in Client.Champions)
            {
                var source = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", c.iconPath),
                    UriKind.Absolute);
                c.icon = new BitmapImage(source);

                if (FreeToPlay != null)
                {
                    c.IsFreeToPlay = FreeToPlay.IsFreeToPlay(c);
                }
                Champions.InsertExtraChampData(c);
            }

            Client.ChampionSkins = (from s in Client.SQLiteDatabase.Table<championSkins>()
                                    orderby s.name
                                    select s).ToList();
            Client.ChampionAbilities = (from s in Client.SQLiteDatabase.Table<championAbilities>()
                                        orderby s.name
                                        select s).ToList();

            Client.SQLiteDatabase.Close();
            Client.Items = Items.PopulateItems();
            Client.Masteries = Masteries.PopulateMasteries();
            Client.Runes = Runes.PopulateRunes();
            if (Client.UpdateRegion != "Garena")
            {
                var reader = new SWFReader(Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"));
                foreach (var tag in reader.Tags)
                {
                    if (!(tag is DoABC))
                        continue;

                    var abcTag = (DoABC) tag;

                    if (!abcTag.Name.Contains("riotgames/platform/gameclient/application/Version"))
                        continue;

                    var str = Encoding.Default.GetString(abcTag.ABCData);
                    var firstSplit = str.Split((char) 6);
                    var secondSplit = firstSplit[0].Split((char) 18);

                    if (secondSplit.Count() > 1)
                    {
                        Client.Version = secondSplit[1];
                    }
                    else
                    {
                        var thirdSplit = secondSplit[0].Split((char)19);
                        Client.Version = thirdSplit[1];
                    }
                }
            }

            Version.Text = Client.Version;

            if (!string.IsNullOrWhiteSpace(Settings.Default.SavedUsername))
            {
                RememberUsernameCheckbox.IsChecked = true;
                LoginUsernameBox.Text = Settings.Default.SavedUsername;
            }
            if (!string.IsNullOrWhiteSpace(Settings.Default.SavedPassword))
            {
                SHA1 sha = new SHA1CryptoServiceProvider();
                RememberPasswordCheckbox.IsChecked = true;
                LoginPasswordBox.Password =
                    Settings.Default.SavedPassword.DecryptStringAES(
                        sha.ComputeHash(Encoding.UTF8.GetBytes(Settings.Default.Guid)).ToString());
            }
            if (!string.IsNullOrWhiteSpace(Settings.Default.Region))
                RegionComboBox.SelectedValue = Settings.Default.Region;

            invisibleLoginCheckBox.IsChecked = Settings.Default.incognitoLogin;
        }

        private void LoginPic_MediaEnded(object sender, RoutedEventArgs e)
        {
            LoginPic.Position = TimeSpan.FromSeconds(0);
            LoginPic.Play();
        }

        private void WaterTextbox_TextChanged(object sender, RoutedEventArgs e)
        {
            Client.Version = Version.Text;
        }

        private bool playingSound = true;

        private void DisableSound_Click(object sender, RoutedEventArgs e)
        {
            if (playingSound)
            {
                SoundPlayer.Pause();
                Sound.IsChecked = true;
                playingSound = false;
            }
            else
            {
                SoundPlayer.Source = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp3"));
                SoundPlayer.Play();
                Sound.IsChecked = false;
                playingSound = true;
            }

            Settings.Default.DisableLoginMusic = Sound.IsChecked.Value;
        }

        private void SoundPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            SoundPlayer.Position = TimeSpan.FromSeconds(0);
            SoundPlayer.Play();
        }

        private bool playingVideo = true;

        private void DisableVideo_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.LoginPageImage == "")
            {
                if (playingVideo)
                {
                    Video.IsChecked = true;
                    playingVideo = false;
                    LoginPic.Source = new System.Uri("http://eddy5641.github.io/LegendaryClient/Login/Login.png");
                }
                else
                {
                    LoginPic.Source = new System.Uri(Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp4"));
                    LoginPic.LoadedBehavior = MediaState.Manual;
                    LoginPic.MediaEnded += LoginPic_MediaEnded;
                    LoginPic.Play();
                    Video.IsChecked = false;
                    playingVideo = true;
                }
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (RegionComboBox.SelectedIndex == -1)
                return;

            if ((string)UpdateRegionComboBox.SelectedValue == "Garena")
            {
                if (RegionComboBox.SelectedIndex == -1)
                    return;
                if (!string.IsNullOrEmpty(RegionComboBox.SelectedValue.ToString()))
                {
                    Settings.Default.DefaultGarenaRegion = RegionComboBox.SelectedValue.ToString(); // Set default Garena region
                    Settings.Default.Save();
                }
                Client.Garena = true;
                await garenaLogin();
                return;
            }
            string UserName = LoginUsernameBox.Text;
            LoggingInLabel.Content = "Logging in...";
            if (string.IsNullOrEmpty(Settings.Default.Guid))
                Settings.Default.Guid = Guid.NewGuid().ToString();

            Settings.Default.Save();
            SHA1 sha = new SHA1CryptoServiceProvider();
            if (RememberPasswordCheckbox.IsChecked == true)
                Settings.Default.SavedPassword =
                    LoginPasswordBox.Password.EncryptStringAES(
                        sha.ComputeHash(Encoding.UTF8.GetBytes(Settings.Default.Guid)).ToString());
            else
                Settings.Default.SavedPassword = "";

            Settings.Default.SavedUsername = RememberUsernameCheckbox.IsChecked == true ? LoginUsernameBox.Text : "";

            if (invisibleLoginCheckBox.IsChecked != null)
                Settings.Default.incognitoLogin = (bool)invisibleLoginCheckBox.IsChecked;

            Settings.Default.Region = (string)RegionComboBox.SelectedValue;
            Settings.Default.Save();

            HideGrid.Visibility = Visibility.Hidden;
            ErrorTextBox.Visibility = Visibility.Hidden;
            LoggingInLabel.Visibility = Visibility.Visible;
            LoggingInProgressRing.Visibility = Visibility.Visible;
            BaseRegion selectedRegion = BaseRegion.GetRegion((string)RegionComboBox.SelectedValue);

            Client.Region = selectedRegion;
            var context = RiotCalls.RegisterObjects();
            Login();
        }

        void client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            ;
        }

        void client_CallbackException(object sender, Exception e)
        {
            throw e;
        }

#pragma warning disable 4014 //Code does not need to be awaited
        async void Login()
        {
            BaseRegion selectedRegion = BaseRegion.GetRegion((string)RegionComboBox.SelectedValue);
            var authToken = await RiotCalls.GetRestToken(LoginUsernameBox.Text, LoginPasswordBox.Password, selectedRegion.LoginQueue);

            if (authToken == "invalid_credentials")
            {
                ErrorTextBox.Text = "Wrong login data";
                HideGrid.Visibility = Visibility.Visible;
                ErrorTextBox.Visibility = Visibility.Visible;
                LoggingInLabel.Visibility = Visibility.Hidden;
                LoggingInProgressRing.Visibility = Visibility.Collapsed;
                return;
            }
            Client.RiotConnection = new RtmpClient(new System.Uri("rtmps://" + selectedRegion.Server + ":2099"), RiotCalls.RegisterObjects(), ObjectEncoding.Amf3);
            Client.RiotConnection.CallbackException += client_CallbackException;
            Client.RiotConnection.MessageReceived += client_MessageReceived;
            try
            {

                await Client.RiotConnection.ConnectAsync();
            }
            catch
            {
                ErrorTextBox.Text = "Failed to login. SSL error.";
                HideGrid.Visibility = Visibility.Visible;
                ErrorTextBox.Visibility = Visibility.Visible;
                LoggingInLabel.Visibility = Visibility.Hidden;
                LoggingInProgressRing.Visibility = Visibility.Collapsed;
                return;
            }

            Client.RiotConnection.SetChunkSize(2147483647);
            AuthenticationCredentials newCredentials = new AuthenticationCredentials
            {
                Username = LoginUsernameBox.Text,
                Password = LoginPasswordBox.Password,
                ClientVersion = Client.Version,
                IpAddress = RiotCalls.GetIpAddress(),
                Locale = selectedRegion.Locale,
                OperatingSystem = "Windows 7",
                Domain = "lolclient.lol.riotgames.com",
                AuthToken = authToken
            };
            Session login = await RiotCalls.Login(newCredentials);
            if (login == null)
            {
                Client.Log("Login session is null.");
                var overlay = new MessageOverlay
                {
                    MessageTextBox = { Text = "Login session is null. Login failed. Please check whether the version number is correct or not.", IsReadOnly = true },
                    MessageTitle = { Content = "Login session is null." }
                };
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
                ErrorTextBox.Text = "Failed to login. Login session is null.";
                HideGrid.Visibility = Visibility.Visible;
                ErrorTextBox.Visibility = Visibility.Visible;
                LoggingInLabel.Visibility = Visibility.Hidden;
                LoggingInProgressRing.Visibility = Visibility.Collapsed;
                return;
            }
            Client.PlayerSession = login;
            var str1 = string.Format("gn-{0}", login.AccountSummary.AccountId);
            var str2 = string.Format("cn-{0}", login.AccountSummary.AccountId);
            var str3 = string.Format("bc-{0}", login.AccountSummary.AccountId);
            Task<bool>[] taskArray = { Client.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str1, str1), 
                                                 Client.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str2, str2), 
                                                 Client.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", "bc", str3) };

            await Task.WhenAll(taskArray);
            //Riot added this for no reason but make it look like the riot client we have to do this
            var plainTextbytes = Encoding.UTF8.GetBytes(login.AccountSummary.Username + ":" + login.Token);
            Client.reconnectToken = Convert.ToBase64String(plainTextbytes);
            //await RiotCalls.Login(result);
            var LoggedIn = await Client.RiotConnection.LoginAsync(LoginUsernameBox.Text.ToLower(), login.Token);
            DoGetOnLoginPacket();
        }

        private async void DoGetOnLoginPacket()
        {
            //TODO: Finish this so all calls are used
            var packetx = await RiotCalls.GetLoginDataPacketForUser();
            Client.Queues = await RiotCalls.GetAvailableQueues();
            Client.PlayerChampions = await RiotCalls.GetAvailableChampions();
            //var runes = await RiotCalls.GetSummonerRuneInventory(packetx.AllSummonerData.Summoner.AcctId);
            Client.StartHeartbeat();
            //var leaguePos = await RiotCalls.GetMyLeaguePositions();
            //var preferences = await RiotCalls.LoadPreferencesByKey();
            //var masterybook = await RiotCalls.GetMasteryBook(packetx.AllSummonerData.Summoner.AcctId);
            //var lobby = await RiotCalls.CheckLobbyStatus();
            var invites = await RiotCalls.GetPendingInvitations();
            //var player = await RiotCalls.CreatePlayer();
            GotLoginPacket(packetx);

            foreach (var pop in from InvitationRequest invite in invites
                                select new GameInvitePopup(invite)
                                {
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                    Height = 230
                                })
                Client.NotificationGrid.Children.Add(pop);
            if (invites.Length != 0)
                Client.MainWin.FlashWindow();

            Client.LoginPacket = packetx;
        }

        private void GotLoginPacket(LoginDataPacket packet)
        {
            if (packet.AllSummonerData == null)
            {
                Client.RiotConnection.CallbackException -= client_CallbackException;
                Client.RiotConnection.MessageReceived -= client_MessageReceived;
                //Just Created Account, need to put logic here.
                Client.done = false;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var createSummoner = new CreateSummonerNameOverlay();
                    Client.OverlayContainer.Content = createSummoner.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                while (!Client.done)
                {
                }
                Login();
                return;
            }
            Client.LoginPacket = packet;
            if (packet.AllSummonerData.Summoner.ProfileIconId == -1)
            {
                Client.RiotConnection.CallbackException -= client_CallbackException;
                Client.RiotConnection.MessageReceived -= client_MessageReceived;
                Client.done = false;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Client.OverlayContainer.Content = new ChooseProfilePicturePage().Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                while (!Client.done)
                {
                }
                Login();

                return;
            }

            Client.RiotConnection.MessageReceived += Client.OnMessageReceived;
            Client.RiotConnection.Disconnected += Client.RiotConnection_Disconnected;
            Client.GameConfigs = packet.GameTypeConfigs;
            Client.IsLoggedIn = true;


            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                MessageBox.Show("Do not play ANY games. I am not sure if they will work ~eddy", "XMPP", MessageBoxButton.OK, MessageBoxImage.Warning);
                Client.StatusContainer.Visibility = Visibility.Visible;
                Client.Container.Margin = new Thickness(0, 0, 0, 40);
                //You have to hand implement this
                //Client.XmppConnection.AutoReconnect = 30;
                Client.XmppConnection = new agsXMPP.XmppClientConnection("pvp.net", 5223)
                {
                    AutoResolveConnectServer = false,
                    ConnectServer = "chat." + Client.Region.ChatName + ".lol.riotgames.com",
                    Resource = "xiff",
                    UseSSL = true,
                    KeepAliveInterval = 10,
                    KeepAlive = true
                };
                Client.XmppConnection.UseCompression = true;
                Client.XmppConnection.OnMessage += Client.XmppConnection_OnMessage;
                Client.XmppConnection.OnError += Client.XmppConnection_OnError;
                Client.XmppConnection.OnLogin += (o) => 
                {
                    Client.Log("Connected to XMPP Server");
                    //Set up chat
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        if (invisibleLoginCheckBox.IsChecked != true)
                            Client.XmppConnection.Send(new Presence(ShowType.chat, Client.GetPresence(), 0) { Type = PresenceType.available });
                        else
                            Client.XmppConnection.Send(new Presence(ShowType.NONE, Client.GetPresence(), 0) { Type = PresenceType.invisible });
                    }));
                };
                Client.RostManager = new RosterManager(Client.XmppConnection);
                Client.XmppConnection.OnRosterItem += Client.RostManager_OnRosterItem;
                Client.XmppConnection.OnRosterEnd += Client.ChatClientConnect;
                Client.PresManager = new PresenceManager(Client.XmppConnection);
                Client.XmppConnection.OnPresence += Client.XmppConnection_OnPresence;
                if (!Client.Garena)
                {
                    Client.userpass = new KeyValuePair<string, string>(LoginUsernameBox.Text,
                        "AIR_" + LoginPasswordBox.Password);
                    
                    Client.XmppConnection.Open(LoginUsernameBox.Text, "AIR_" + LoginPasswordBox.Password);

                    //Client.XmppConnection.OnInvalidCertificate += Client.XmppConnection_OnInvalidCertificate;
                }
                else
                {
                    Client.XmppConnection.ConnectServer = "chat" + Client.Region.ChatName + ".lol.garenanow.com";
                    var gas = getGas();
                    Client.XmppConnection.Open(Client.UID, "AIR_" + gas);
                    Client.userpass = new KeyValuePair<string, string>(Client.UID, "AIR_" + gas);
                }

                //Client.PresManager.OnPrimarySessionChange += Client.PresManager_OnPrimarySessionChange;
                /*
                Client.ConfManager = new ConferenceManager
                {
                    Stream = Client.XmppConnection
                };
                //*/
                //switch
                Client.Log("Connected to " + Client.Region.RegionName + " and logged in as " +
                           Client.LoginPacket.AllSummonerData.Summoner.Name);

                //Gather data and convert it that way that it does not cause errors
                PlatformGameLifecycleDTO data = (PlatformGameLifecycleDTO)Client.LoginPacket.ReconnectInfo;

                Client.MainPage = new MainPage();
                if (data != null && data.Game != null)
                {
                    Client.Log(data.PlayerCredentials.ChampionId.ToString(CultureInfo.InvariantCulture));
                    Client.CurrentGame = data.PlayerCredentials;
                    Client.GameType = data.Game.GameType;
                    Client.SwitchPage(new InGame());
                }
                else
                    Client.SwitchPage(Client.MainPage);

                Client.ClearPage(typeof(LoginPage));
            }));
        }

        public static string GetNewIpAddress()
        {
            var sb = new StringBuilder();

            WebRequest con = WebRequest.Create("http://ll.leagueoflegends.com/services/connection_info");
            WebResponse response = con.GetResponse();

            int c;
            Stream responseStream = response.GetResponseStream();
            while (responseStream != null && (c = responseStream.ReadByte()) != -1)
                sb.Append((char)c);

            con.Abort();

            var serializer = new JavaScriptSerializer();
            var deserializedJson = serializer.Deserialize<Dictionary<string, string>>(sb.ToString());
            try
            {
                if (deserializedJson.Any(x => x.Key.Contains("403") || x.Value.Contains("403")))
                    throw new HttpListenerException(403);

                return deserializedJson["ip_address"];
            }
            catch
            {
                string localIp = "?";
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList.Where(ip => ip.AddressFamily.ToString() == "InterNetwork"))
                    localIp = ip.ToString();

                return localIp;
            }
        }

        private Vector moveOffset;
        private Point _currentLocation;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            _currentLocation = Mouse.GetPosition(MouseGrid);
            moveOffset = new Vector(Tt.X, Tt.Y);
            HideGrid.CaptureMouse();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!HideGrid.IsMouseCaptured)
                return;

            Vector offset = Point.Subtract(e.GetPosition(MouseGrid), _currentLocation);
            Tt.X = moveOffset.X + offset.X;
            Tt.Y = moveOffset.Y + offset.Y;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HideGrid.ReleaseMouseCapture();
        }

        private async Task garenaLogin()
        {
            WindowsIdentity winIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal winPrincipal = new WindowsPrincipal(winIdentity);
            if (!winPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                if (await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync("Insufficent Privledges.", "Press OK to restart as admin.") == MessageDialogResult.Affirmative)
                {
                    var info = new ProcessStartInfo(Path.Combine(Client.ExecutingDirectory, "Client", "LegendaryClient.exe"))
                    {
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    Process.Start(info);
                    Environment.Exit(0);
                }
            }

            LoggingInLabel.Content = "Waiting for user to launch League from garena";
            HideGrid.Visibility = Visibility.Hidden;
            ErrorTextBox.Visibility = Visibility.Hidden;
            LoggingInLabel.Visibility = Visibility.Visible;
            var garenaregion = BaseRegion.GetRegion((string)RegionComboBox.SelectedValue);
            LoggingInProgressRing.Visibility = Visibility.Visible;
            LoginPasswordBox.Visibility = Visibility.Hidden;
            shouldExit = false;
            while (!shouldExit)
            {
                await Task.Delay(500);
                foreach (var process in Process.GetProcessesByName("lol"))
                {
                    var s1 = GetCommandLine(process);
                    process.Kill();
                    foreach (var lolclient in Process.GetProcessesByName("LolClient"))
                    {
                        lolclient.Kill();
                    }
                    if (s1.StartsWith("\""))
                    {
                        try
                        {
                            s1 = s1.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];
                        }
                        catch
                        {
                            Client.Log("Error splitting path from command line.");
                        }
                    }
                    else
                    {
                        s1 = s1.Substring(1);
                    }
                    Client.Log("Received token, it is: " + s1);
                    Client.Region = garenaregion;
                    Dispatcher.BeginInvoke(
                        DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                HideGrid.Visibility = Visibility.Hidden;
                                ErrorTextBox.Visibility = Visibility.Hidden;
                                LoggingInLabel.Visibility = Visibility.Visible;
                                LoggingInLabel.Content = "Logging in...";
                                LoggingInProgressRing.Visibility = Visibility.Visible;
                            }));
                    var context = RiotCalls.RegisterObjects();
                    Client.RiotConnection = new RtmpClient(new System.Uri("rtmps://" + garenaregion.Server + ":2099"), context, ObjectEncoding.Amf3);
                    Client.RiotConnection.CallbackException += client_CallbackException;
                    Client.RiotConnection.MessageReceived += client_MessageReceived;
                    await Client.RiotConnection.ConnectAsync();
                    Client.RiotConnection.SetChunkSize(2147483647);

                    AuthenticationCredentials newCredentials = new AuthenticationCredentials
                    {
                        AuthToken = await
                            RiotCalls.GetRestToken(LoginUsernameBox.Text, LoginPasswordBox.Password, garenaregion.LoginQueue, reToken(s1)),
                        Username = Client.UID,
                        Password = null,
                        ClientVersion = Client.Version,
                        IpAddress = RiotCalls.GetIpAddress(),
                        Locale = garenaregion.Locale,
                        PartnerCredentials = "8393 " + s1,
                        OperatingSystem = "Windows 7",
                        Domain = "lolclient.lol.riotgames.com",
                    };
                    Session login = await RiotCalls.Login(newCredentials);
                    Client.PlayerSession = login;
                    var str1 = string.Format("gn-{0}", login.AccountSummary.AccountId);
                    var str2 = string.Format("cn-{0}", login.AccountSummary.AccountId);
                    var str3 = string.Format("bc-{0}", login.AccountSummary.AccountId);
                    Task<bool>[] taskArray = { Client.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str1, str1), 
                                                 Client.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str2, str2), 
                                                 Client.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", "bc", str3) };

                    await Task.WhenAll(taskArray);
                    var LoggedIn = await Client.RiotConnection.LoginAsync(Client.UID, login.Token);
                    //var packet = await RiotCalls.GetLoginDataPacketForUser();
                    DoGetOnLoginPacket();
                }
            }
        }

        private static string GetCommandLine(Process process)
        {
            var commandLine = new StringBuilder("");

            using (var searcher = new ManagementObjectSearcher("SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
            {
                foreach (var @object in searcher.Get())
                {
                    commandLine.Append(@object["CommandLine"]);
                }
            }

            return commandLine.ToString();
        }

        private void UpdateRegionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count != 0)
            {
                Settings.Default.updateRegion = (string)UpdateRegionComboBox.SelectedValue;


                Client.UpdateRegion = (string)UpdateRegionComboBox.SelectedValue;
                if (!RegionComboBox.Items.IsInUse)
                {
                    RegionComboBox.Items.Clear();
                    Thread.Sleep(100);
                }
                switch (Client.UpdateRegion)
                {
                    case "PBE":
                        RegionComboBox.ItemsSource = new[] { "PBE" };
                        LoginUsernameBox.Visibility = Visibility.Visible;
                        LoginPasswordBox.Visibility = Visibility.Visible;
                        RememberUsernameCheckbox.Visibility = Visibility.Visible;
                        break;

                    case "Live":
                        RegionComboBox.ItemsSource = new[] { "BR", "EUNE", "EUW", "NA", "OCE", "RU", "LAS", "LAN", "TR", "CS" };
                        LoginUsernameBox.Visibility = Visibility.Visible;
                        LoginPasswordBox.Visibility = Visibility.Visible;
                        RememberUsernameCheckbox.Visibility = Visibility.Visible;
                        break;

                    case "Korea":
                        RegionComboBox.ItemsSource = new[] { "KR" };
                        LoginUsernameBox.Visibility = Visibility.Visible;
                        RememberUsernameCheckbox.Visibility = Visibility.Visible;
                        LoginPasswordBox.Visibility = Visibility.Visible;
                        break;

                    case "Garena":
                        RegionComboBox.ItemsSource = new[] { "PH", "SG", "SGMY", "TH", "TW", "VN", "ID" };
                        LoginUsernameBox.Visibility = Visibility.Hidden;
                        RememberUsernameCheckbox.Visibility = Visibility.Hidden;
                        LoginPasswordBox.Visibility = Visibility.Hidden;
                        break;
                }
            }
        }

        public string getGas()
        {

            //string begin = "{\"signature\":\"";
            //string end = "}";
            string gas = Client.Gas;
            gas = gas.Replace("\r\n  ", "");
            byte[] encbuff = Encoding.UTF8.GetBytes(gas);
            gas = HttpServerUtility.UrlTokenEncode(encbuff);

            return gas;
        }

        private static string reToken(string s)
        {
            var s1 = s.Replace("/", "%2F");
            s1 = s1.Replace("+", "%2B");
            s1 = s1.Replace("=", "%3D");

            return s1;
        }

        private void AutoLoginCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            if ((AutoLoginCheckBox.IsChecked == true && RememberUsernameCheckbox.IsChecked == true && RememberPasswordCheckbox.IsChecked == true) || Client.Garena || (string)UpdateRegionComboBox.SelectedValue == "Garena")
            {
                Settings.Default.AutoLogin = true;
            }
            else
            {
                RememberPasswordCheckbox.IsChecked = true;
                RememberUsernameCheckbox.IsChecked = true;
                Settings.Default.AutoLogin = true;
            }

            Settings.Default.Save();
        }

        private void MouseGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new ThreadStart(() => {
                AutoLoginCheckBox.IsChecked = Settings.Default.AutoLogin;
            }), DispatcherPriority.Input);

            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 3000; 
            timer.Elapsed += (s, a) =>
            {
                timer.Stop();

                bool autoLogin = false;
                Dispatcher.Invoke(() => {
                    autoLogin = (bool)AutoLoginCheckBox.IsChecked;
                });

                if (autoLogin)
                {
                    Client.Log("Auto login");
                    this.Dispatcher.BeginInvoke(new ThreadStart(() => {
                        LoginButton_Click(null, null);
                    }), DispatcherPriority.Input);
                }
            };
            timer.Start();
        }
    }
}
