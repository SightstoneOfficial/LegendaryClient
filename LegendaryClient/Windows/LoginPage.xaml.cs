﻿#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using jabber.client;
using jabber.connection;
using LegendaryClient.Logic;
using LegendaryClient.Logic.JSON;
using LegendaryClient.Logic.Region;
using LegendaryClient.Logic.SQLite;
using LegendaryClient.Logic.SWF;
using LegendaryClient.Logic.SWF.SWFTypes;
using LegendaryClient.Logic.UpdateRegion;
using LegendaryClient.Properties;
using LegendaryClient.Logic.Patcher;
using Microsoft.Win32;
using PVPNetConnect;
using PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Login;
using SQLite;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage
    {
        public LoginPage()
        {
            InitializeComponent();
            Change();
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
                case "PBE": RegionComboBox.ItemsSource = new[] { "PBE" };
                    LoginUsernameBox.Visibility = Visibility.Visible;
                    LoginUsername.Visibility = Visibility.Visible;
                    RememberUsernameCheckbox.Visibility = Visibility.Visible;
                    LoginPassword.Visibility = Visibility.Visible;
                    break;

                case "Live": RegionComboBox.ItemsSource = new[] { "BR", "EUNE", "EUW", "NA", "OCE", "RU", "LAS", "LAN", "TR", "CS" };
                    LoginUsernameBox.Visibility = Visibility.Visible;
                    LoginUsername.Visibility = Visibility.Visible;
                    RememberUsernameCheckbox.Visibility = Visibility.Visible;
                    LoginPassword.Visibility = Visibility.Visible;
                    break;

                case "Korea": RegionComboBox.ItemsSource = new[] { "KR" };
                    LoginUsernameBox.Visibility = Visibility.Visible;
                    LoginUsername.Visibility = Visibility.Visible;
                    RememberUsernameCheckbox.Visibility = Visibility.Visible;
                    LoginPassword.Visibility = Visibility.Visible;
                    break;

                case "Garena": RegionComboBox.ItemsSource = new[] { "PH", "SG", "SGMY", "TH", "TW", "VN" };
                    LoginUsernameBox.Visibility = Visibility.Hidden;
                    LoginUsername.Visibility = Visibility.Hidden;
                    RememberUsernameCheckbox.Visibility = Visibility.Hidden;
                    LoginPassword.Visibility = Visibility.Hidden;
                    break;
            }


            if (!Settings.Default.DisableLoginMusic)
            {
                SoundPlayer.Source = new Uri(Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp3"));
                SoundPlayer.Play();
                Sound.IsChecked = false;
            }
            else Sound.IsChecked = true;

            if (!String.IsNullOrEmpty(Settings.Default.devKeyLoc))
            {
                if (Client.Authenticate(Settings.Default.devKeyLoc))
                {
                    Client.Dev = true;
                    devKeyLabel.Content = "Dev";
                }
            }

            if (Settings.Default.LoginPageImage == "")
            {
                LoginPic.Source = new Uri(Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp4"));
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
                            new Uri(
                                Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                                    Settings.Default.LoginPageImage), UriKind.Absolute));
            }

            Video.IsChecked = false;

            //Get client data after patcher completed

            Client.SQLiteDatabase = new SQLiteConnection(Path.Combine(Client.ExecutingDirectory, Client.sqlite));
            Client.Champions = (from s in Client.SQLiteDatabase.Table<champions>()
                                orderby s.name
                                select s).ToList();

            FreeToPlayChampions.GetInstance();

            foreach (champions c in Client.Champions)
            {
                var source = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", c.iconPath),
                    UriKind.Absolute);
                c.icon = new BitmapImage(source);
                Debugger.Log(0, "Log", "Requesting :" + c.name + " champ");
                Debugger.Log(0, "", Environment.NewLine);

                try
                {
                    c.IsFreeToPlay = FreeToPlayChampions.GetInstance().IsFreeToPlay(c);
                    Champions.InsertExtraChampData(c);
                    //why was this ever here? all of the needed info is already in the sqlite file
                }
                catch
                {
                    Client.Log("error, file not found", "NotFound");
                }
            }
            Client.ChampionSkins = (from s in Client.SQLiteDatabase.Table<championSkins>()
                                    orderby s.name
                                    select s).ToList();
            Client.ChampionAbilities = (from s in Client.SQLiteDatabase.Table<championAbilities>()
                                        //Needs Fixed
                                        orderby s.name
                                        select s).ToList();
            Client.SearchTags = (from s in Client.SQLiteDatabase.Table<championSearchTags>()
                                 orderby s.id
                                 select s).ToList();
            Client.Keybinds = (from s in Client.SQLiteDatabase.Table<keybindingEvents>()
                               orderby s.id
                               select s).ToList();
            Client.Items = Items.PopulateItems();
            Client.Masteries = Masteries.PopulateMasteries();
            Client.Runes = Runes.PopulateRunes();
            BaseUpdateRegion updateRegion = BaseUpdateRegion.GetUpdateRegion(Client.UpdateRegion);
            var patcher = new RiotPatcher();

            string tempString = patcher.GetListing(updateRegion.AirListing);

            string[] packages = patcher.GetManifest(
                updateRegion.AirManifest + "releases/" + tempString + "/packages/files/packagemanifest");
            foreach (
                string usestring in
                    packages.Select(package => package.Split(',')[0])
                        .Where(usestring => usestring.Contains("ClientLibCommon.dat")))
                new WebClient().DownloadFile(new Uri(updateRegion.BaseLink + usestring),
                    Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"));

            var reader = new SWFReader(Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"));
            foreach (var secondSplit in from abcTag in reader.Tags.OfType<DoABC>()
                                        where abcTag.Name.Contains("riotgames/platform/gameclient/application/Version")
                                        select Encoding.Default.GetString(abcTag.ABCData)
                                            into str
                                            select str.Split((char)6)
                                                into firstSplit

                                                select firstSplit[0].Split((char)18))

                try
                {
                    Client.Version = secondSplit[1];
                }
                catch
                {
                    var thirdSplit = secondSplit[0].Split((char)19);
                    Client.Version = thirdSplit[1];
                }


            Version.Text = Client.Version;

            if (!String.IsNullOrWhiteSpace(Settings.Default.SavedUsername))
            {
                RememberUsernameCheckbox.IsChecked = true;
                LoginUsernameBox.Text = Settings.Default.SavedUsername;
            }
            if (!String.IsNullOrWhiteSpace(Settings.Default.SavedPassword))
            {
                SHA1 sha = new SHA1CryptoServiceProvider();
                RememberPasswordCheckbox.IsChecked = true;
                LoginPasswordBox.Password =
                    Settings.Default.SavedPassword.DecryptStringAES(
                        sha.ComputeHash(Encoding.UTF8.GetBytes(Settings.Default.Guid)).ToString());
            }
            if (!String.IsNullOrWhiteSpace(Settings.Default.Region))
                RegionComboBox.SelectedValue = Settings.Default.Region;

            invisibleLoginCheckBox.IsChecked = Settings.Default.incognitoLogin;

            /*var uriSource =
                new Uri(
                    Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                        champions.GetChampion(Client.LatestChamp).splashPath), UriKind.Absolute);
            LoginImage.Source = new BitmapImage(uriSource);*/

            if (String.IsNullOrWhiteSpace(Settings.Default.SavedPassword) ||
                String.IsNullOrWhiteSpace(Settings.Default.Region) || !Settings.Default.AutoLogin)
                return;

            AutoLoginCheckBox.IsChecked = true;
            LoginButton_Click(null, null);
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
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

        private bool _playingSound = true;

        private void DisableSound_Click(object sender, RoutedEventArgs e)
        {
            if (_playingSound)
            {
                SoundPlayer.Pause();
                Sound.IsChecked = true;
                _playingSound = false;
            }
            else
            {
                SoundPlayer.Source = new Uri(Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp3"));
                SoundPlayer.Play();
                Sound.IsChecked = false;
                _playingSound = true;
            }

            Settings.Default.DisableLoginMusic = Sound.IsChecked.Value;
        }

        private void SoundPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            SoundPlayer.Position = TimeSpan.FromSeconds(0);
            SoundPlayer.Play();
        }

        private bool _playingVideo = true;

        private void DisableVideo_Click(object sender, RoutedEventArgs e)
        {
            if (Settings.Default.LoginPageImage == "")
            {
                if (_playingVideo)
                {
                    Video.IsChecked = true;
                    _playingVideo = false;
                    try
                    {
                        LoginPic.Source = new Uri("http://eddy5641.github.io/LegendaryClient/Login/Login.png");
                    }
                    catch
                    {
                    }
                }
                else
                {
                    LoginPic.Source = new Uri(Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp4"));
                    LoginPic.LoadedBehavior = MediaState.Manual;
                    LoginPic.MediaEnded += LoginPic_MediaEnded;
                    LoginPic.Play();
                    Video.IsChecked = false;
                    _playingVideo = true;
                }
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if ((string) UpdateRegionComboBox.SelectedValue == "Garena")
            {
                if(String.IsNullOrEmpty(LoginPasswordBox.Password))
                {
                    SniffGarena((string)RegionComboBox.SelectedValue);
                }
                else
                {
                    LoginGarena(LoginPasswordBox.Password, (string)RegionComboBox.SelectedValue);
                }
                return;
            }
            Client.PVPNet = null;
            Client.PVPNet = new PVPNetConnection();
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

            if (AutoLoginCheckBox.IsChecked != null)
                Settings.Default.AutoLogin = (bool)AutoLoginCheckBox.IsChecked;

            if (invisibleLoginCheckBox.IsChecked != null)
                Settings.Default.incognitoLogin = (bool)invisibleLoginCheckBox.IsChecked;

            Settings.Default.Region = (string)RegionComboBox.SelectedValue;
            Settings.Default.Save();

            HideGrid.Visibility = Visibility.Hidden;
            ErrorTextBox.Visibility = Visibility.Hidden;
            LoggingInLabel.Visibility = Visibility.Visible;
            LoggingInProgressRing.Visibility = Visibility.Visible;
            Client.PVPNet.OnError += PVPNet_OnError;
            Client.PVPNet.OnLogin += PVPNet_OnLogin;
            Client.PVPNet.OnMessageReceived += Client.OnMessageReceived;
            BaseRegion selectedRegion = BaseRegion.GetRegion((string)RegionComboBox.SelectedValue);

            Client.Region = selectedRegion;
            if (selectedRegion.PVPRegion != Region.CS)
                Client.PVPNet.Connect(LoginUsernameBox.Text, LoginPasswordBox.Password, selectedRegion.PVPRegion,
                    Client.Version);
            else
            {
                Dictionary<String, String> settings = selectedRegion.Location.LeagueSettingsReader();
                Client.PVPNet.Connect(LoginUsernameBox.Text, LoginPasswordBox.Password, selectedRegion.PVPRegion,
                    Client.Version, true, settings["host"], settings["lq_uri"], selectedRegion.Locale);
            }
        }

        private void PVPNet_OnLogin(object sender, string username, string ipAddress)
        {
            Client.PVPNet.GetLoginDataPacketForUser(GotLoginPacket);
        }

        private void PVPNet_OnError(object sender, Error error)
        {
            //Display error message
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                HideGrid.Visibility = Visibility.Visible;
                ErrorTextBox.Visibility = Visibility.Visible;
                LoggingInProgressRing.Visibility = Visibility.Hidden;
                LoggingInLabel.Visibility = Visibility.Hidden;
                ErrorTextBox.Text = error.Message;
            }));
            Client.PVPNet.OnMessageReceived -= Client.OnMessageReceived;
            Client.PVPNet.OnError -= PVPNet_OnError;
            Client.PVPNet.OnLogin -= PVPNet_OnLogin;
        }

#pragma warning disable 4014 //Code does not need to be awaited


        private async void GotLoginPacket(LoginDataPacket packet)
        {
            if (packet.AllSummonerData == null)
            {
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
                Client.PVPNet.Connect(LoginUsernameBox.Text, LoginPasswordBox.Password, Client.Region.PVPRegion,
                    Client.Version);
                return;
            }
            Client.LoginPacket = packet;
            if (packet.AllSummonerData.Summoner.ProfileIconId == -1)
            {
                Client.done = false;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Client.OverlayContainer.Content = new ChooseProfilePicturePage().Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                while (!Client.done)
                {
                }
                Client.PVPNet.Connect(LoginUsernameBox.Text, LoginPasswordBox.Password, Client.Region.PVPRegion,
                    Client.Version);

                return;
            }
            Client.PlayerChampions = await Client.PVPNet.GetAvailableChampions();
            Client.PVPNet.OnError -= PVPNet_OnError;
            Client.GameConfigs = packet.GameTypeConfigs;
            Client.PVPNet.Subscribe("bc", packet.AllSummonerData.Summoner.AcctId);
            Client.PVPNet.Subscribe("cn", packet.AllSummonerData.Summoner.AcctId);
            Client.PVPNet.Subscribe("gn", packet.AllSummonerData.Summoner.AcctId);
            Client.IsLoggedIn = true;


            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                Client.StatusContainer.Visibility = Visibility.Visible;
                Client.Container.Margin = new Thickness(0, 0, 0, 40);

                var newCredentials = new AuthenticationCredentials
                {
                    Username = LoginUsernameBox.Text,
                    Password = LoginPasswordBox.Password,
                    ClientVersion = Client.Version,
                    IpAddress = GetNewIpAddress(),
                    Locale = Client.Region.Locale,
                    Domain = "lolclient.lol.riotgames.com"
                };
                //Almost like the lol client now
                string os = Environment.OSVersion.ToString();
                string[] ossplit = os.Split('.');
                if (ossplit[0] == "Windows 8")
                {
                    if (ossplit[1] == "1")
                        os = "Windows 8.1";
                }
                else
                    os = ossplit[0];

                newCredentials.OperatingSystem = os;

                Session login = await Client.PVPNet.Login(newCredentials);
                Client.PlayerSession = login;

                //Setup chat
                if (!Client.Garena)
                {
                    Client.ChatClient.AutoReconnect = 30;
                    Client.ChatClient.KeepAlive = 10;
                    Client.ChatClient.NetworkHost = "chat." + Client.Region.ChatName + ".lol.riotgames.com";
                    Client.ChatClient.Port = 5223;
                    Client.ChatClient.Server = "pvp.net";
                    Client.ChatClient.SSL = true;
                    Client.ChatClient.User = LoginUsernameBox.Text;
                    Client.ChatClient.Password = "AIR_" + LoginPasswordBox.Password;
                    Client.userpass = new KeyValuePair<String, String>(LoginUsernameBox.Text,
                        "AIR_" + LoginPasswordBox.Password);
                    Client.ChatClient.OnInvalidCertificate += Client.ChatClient_OnInvalidCertificate;
                    Client.ChatClient.OnMessage += Client.ChatClient_OnMessage;
                    Client.ChatClient.OnPresence += Client.ChatClient_OnPresence;
                    Client.ChatClient.OnDisconnect += Client.ChatClient_OnDisconnect;
                    Client.ChatClient.Connect();
                }
                else
                {
                    Client.ChatClient.AutoReconnect = 30;
                    Client.ChatClient.KeepAlive = 10;
                    Client.ChatClient.NetworkHost = "chat" + Client.Region.ChatName + ".lol.garenanow.com";
                    Client.ChatClient.Port = 5223;
                    Client.ChatClient.Server = "pvp.net";
                    Client.ChatClient.SSL = true;
                    Client.ChatClient.User = Client.PVPNet.getUID();
                    var gas = Client.PVPNet.getGas();
                    Client.ChatClient.Password = "AIR_" + gas;
                    Client.userpass = new KeyValuePair<String, String>(Client.PVPNet.getUID(),
                        "AIR_" + gas);
                    Client.ChatClient.OnInvalidCertificate += Client.ChatClient_OnInvalidCertificate;
                    Client.ChatClient.OnMessage += Client.ChatClient_OnMessage;
                    Client.ChatClient.OnPresence += Client.ChatClient_OnPresence;
                    Client.ChatClient.OnDisconnect += Client.ChatClient_OnDisconnect;
                    Client.ChatClient.Connect();
                }

                Client.RostManager = new RosterManager
                {
                    Stream = Client.ChatClient,
                    AutoAllow = AutoSubscriptionHanding.AllowAll
                };
                Client.RostManager.OnRosterItem += Client.RostManager_OnRosterItem;
                Client.RostManager.OnRosterEnd += Client.ChatClientConnect;

                Client.PresManager = new PresenceManager
                {
                    Stream = Client.ChatClient
                };
                Client.PresManager.OnPrimarySessionChange += Client.PresManager_OnPrimarySessionChange;

                Client.ConfManager = new ConferenceManager
                {
                    Stream = Client.ChatClient
                };
                //switch
                Client.Log("Connected to " + Client.Region.RegionName + " and logged in as " +
                           Client.LoginPacket.AllSummonerData.Summoner.Name);

                //Gather data and convert it that way that it does not cause errors
                PlatformGameLifecycleDTO data = Client.LoginPacket.ReconnectInfo;

                if (data != null && data.Game != null)
                {
                    Client.Log(data.PlayerCredentials.ChampionId.ToString(CultureInfo.InvariantCulture));
                    Client.CurrentGame = data.PlayerCredentials;
                    Client.GameType = data.Game.GameType;
                    Client.SwitchPage(new InGame());
                }
                else
                    uiLogic.UpdateMainPage();

                Client.ClearPage(typeof(LoginPage));
            }));
        }

        public string GetLast(string source, int tail_length)
        {
            if (tail_length >= source.Length)
                return source;
            return source.Substring(source.Length - tail_length);
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

        private Vector _moveOffset;
        private Point _currentLocation;

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            _currentLocation = Mouse.GetPosition(MouseGrid);
            _moveOffset = new Vector(Tt.X, Tt.Y);
            HideGrid.CaptureMouse();
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!HideGrid.IsMouseCaptured)
                return;

            Vector offset = Point.Subtract(e.GetPosition(MouseGrid), _currentLocation);
            Tt.X = _moveOffset.X + offset.X;
            Tt.Y = _moveOffset.Y + offset.Y;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            HideGrid.ReleaseMouseCapture();
        }

        private void devKeyLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Client.Authenticate(null))
            {
                Client.Dev = true;
                devKeyLabel.Content = "Dev";
                MessageBox.Show("Dev mode activated.",
                    "LC Notification",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                Client.Dev = false;
                devKeyLabel.Content = "User";
                MessageBox.Show("Failed to approve key",
                    "LC Notification",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        //This is to avoid replacing Garena, this is a better method
        private void SniffGarena(String region)
        {
            LoggingInLabel.Content = "Waiting for user to launch League from garena";
            HideGrid.Visibility = Visibility.Hidden;
            ErrorTextBox.Visibility = Visibility.Hidden;
            LoggingInLabel.Visibility = Visibility.Visible;
            LoggingInProgressRing.Visibility = Visibility.Visible;
            var gotToken = false;
            var getTokenThread = new Thread(() =>
                {
                    while (!gotToken)
                    {
                        foreach (var process in Process.GetProcessesByName("lol"))
                        {
                            try
                            {
                                var s1 = GetCommandLine(process);
                                foreach (var p1 in Process.GetProcessesByName("lolclient"))
                                {
                                    p1.Kill();
                                }
                                process.Kill();
                                gotToken = true;

                                s1 = s1.Substring(1);
                                if (!gotToken)
                                {
                                    continue;
                                }
                                gotToken = !gotToken;
                                Client.PVPNet = null;
                                Client.PVPNet = new PVPNetConnection();
                                var garenaregion = BaseRegion.GetRegion(region);
                                Client.PVPNet.garenaToken = s1;
                                Client.PVPNet.Connect("", "", garenaregion.PVPRegion, Client.Version);
                                Client.Region = garenaregion;
                                LoggingInLabel.Dispatcher.Invoke(new Action(() => LoggingInLabel.Content = "Logging in..."));
                                Client.PVPNet.OnError += PVPNet_OnError;
                                Client.PVPNet.OnLogin += PVPNet_OnLogin;
                                Client.PVPNet.OnMessageReceived += Client.OnMessageReceived;
                            }
                            catch { }
                        }
                    }
                });
            getTokenThread.Start();

        }

        //Garena login with custom token
        private void LoginGarena(String token, String region)
        {
            LoggingInLabel.Content = "Logging in using entered token...";
            HideGrid.Visibility = Visibility.Hidden;
            ErrorTextBox.Visibility = Visibility.Hidden;
            LoggingInLabel.Visibility = Visibility.Visible;
            LoggingInProgressRing.Visibility = Visibility.Visible;
            var LoginThread = new Thread(() =>
            {
     
                            token = token.Substring(1);
                            Client.PVPNet = null;
                            Client.PVPNet = new PVPNetConnection();
                            var garenaregion = BaseRegion.GetRegion(region);
                            Client.PVPNet.garenaToken = token;
                            Client.PVPNet.Connect("", "", garenaregion.PVPRegion, Client.Version);
                            Client.Region = garenaregion;
                            Client.PVPNet.OnError += PVPNet_OnError;
                            Client.PVPNet.OnLogin += PVPNet_OnLogin;
                            Client.PVPNet.OnMessageReceived += Client.OnMessageReceived;
            });
            LoginThread.Start();
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
                    case "PBE": RegionComboBox.ItemsSource = new[] { "PBE" };
                        LoginUsernameBox.Visibility = Visibility.Visible;
                        LoginUsername.Visibility = Visibility.Visible;
                        RememberUsernameCheckbox.Visibility = Visibility.Visible;
                        break;

                    case "Live": RegionComboBox.ItemsSource = new[] { "BR", "EUNE", "EUW", "NA", "OCE", "RU", "LAS", "LAN", "TR", "CS" };
                        LoginUsernameBox.Visibility = Visibility.Visible;
                        LoginUsername.Visibility = Visibility.Visible;
                        RememberUsernameCheckbox.Visibility = Visibility.Visible;
                        break;

                    case "Korea": RegionComboBox.ItemsSource = new[] { "KR" };
                        LoginUsernameBox.Visibility = Visibility.Visible;
                        LoginUsername.Visibility = Visibility.Visible;
                        RememberUsernameCheckbox.Visibility = Visibility.Visible;
                        break;

                    case "Garena": RegionComboBox.ItemsSource = new[] { "PH", "SG", "SGMY", "TH", "TW", "VN" };
                        LoginUsernameBox.Visibility = Visibility.Hidden;
                        LoginUsername.Visibility = Visibility.Hidden;
                        RememberUsernameCheckbox.Visibility = Visibility.Hidden;
                        LoginPassword.Text = "Garena Token";
                        break;
                }
            }
        }
    }
}