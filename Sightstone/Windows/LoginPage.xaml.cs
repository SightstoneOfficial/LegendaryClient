using Sightstone.Logic.JSON;
using Sightstone.Logic.Region;
using Sightstone.Logic.SQLite;
using Sightstone.Logic.SWF;
using Sightstone.Logic.SWF.SWFTypes;
using Sightstone.Logic.UpdateRegion;
using Sightstone.Properties;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Sightstone.Controls;
using Sightstone.Logic.Riot;
using Sightstone.Logic.Riot.Platform;
using RtmpSharp.IO;
using RtmpSharp.Messaging;
using RtmpSharp.Net;
using Brush = System.Windows.Media.Brush;
using Point = System.Windows.Point;
using RiotPatcher = Sightstone.Logic.Patcher.RiotPatcher;
using agsXMPP.protocol.iq.roster;
using agsXMPP.protocol.client;
using System.Security.Principal;
using System.Security.Cryptography;
using Microsoft.Win32;
using Sightstone.Logic.MultiUser;
using System.ComponentModel;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage
    {
        private bool shouldExit;
        private bool authed;
        bool switchpage;
        Dictionary<string, LoginData> dataLogin = new Dictionary<string, LoginData>();
        private bool saveuser;

        public LoginPage()
        {
            InitializeComponent();
            string lolRootPath = GetLolRootPath(false);
            Client.Log("League of Legends is located at: " + lolRootPath);
            //RADS\solutions\lol_game_client_sln\releases
            string gameLocation = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln",
                "releases");
            string solutionListing = new RiotPatcher().GetListing("http://l3cdn.riotgames.com/releases/live/solutions/lol_game_client_sln/releases/releaselisting_NA");

            string solutionVersion = solutionListing.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
            Client.GameClientVersion = solutionVersion;
            Client.Location = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln",
                            "releases", solutionVersion, "deploy");

            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", "themedata")))
                Client.Theme = File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", "themedata"))[0];

            if (File.Exists(Path.Combine(Settings.Default.GarenaLocation, "Air", "Lib", "ClientLibCommon.dat")))
            {
                File.Copy(Path.Combine(Settings.Default.GarenaLocation, "Air", "Lib", "ClientLibCommon.dat"),
                          Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"), true);

                Client.Log("Garena is Up-To-Date");
                Client.GLocation = Path.Combine(lolRootPath.Replace("lol.exe", ""), "Game");
                Client.GRootLocation = lolRootPath;
            }
            if (Directory.Exists(Path.Combine(gameLocation, solutionVersion)))
            {
                Client.Log("League of Legends is Up-To-Date");
                Client.Location = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln",
                    "releases", solutionVersion, "deploy");
                Client.RootLocation = lolRootPath;
            }

            Client.CurrentServer = "NA";
            Version.TextChanged += WaterTextbox_TextChanged;
            bool x = Settings.Default.DarkTheme;
            if (!x)
            {
                var bc = new BrushConverter();
                HideGrid.Background = (Brush)bc.ConvertFrom("#B24F4F4F");
                LoggingInProgressRing.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
            }
            //#B2C8C8C8
            UpdateRegionComboBox.ItemsSource = new[] { "PBE", "Live", "Korea", "Garena" };

            UpdateRegionComboBox.SelectedIndex = 1;
            //switch (user.Instance.UpdateRegion)
            switch (UpdateRegionComboBox.SelectedItem.ToString())
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

            string themeLocation = "";
            if (!String.IsNullOrEmpty(Client.Theme))
                themeLocation = Path.Combine(Client.ExecutingDirectory, "assets", "themes", Client.Theme);

            if (!Settings.Default.DisableLoginMusic)
            {
                string[] music = null;
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
                    if (!String.IsNullOrEmpty(themeLocation))
                        music = Directory.GetFiles(themeLocation, "*.mp3");
                }
                if (music != null && music.Length > 0 && !String.IsNullOrEmpty(themeLocation))
                {
                    SoundPlayer.Source = new System.Uri(Path.Combine(themeLocation, music[0]));
                    SoundPlayer.Play();
                }
                if (Settings.Default.LoginMusicVolume != -1)
                {
                    slider.Value = Settings.Default.LoginMusicVolume;
                    SoundPlayer.Volume = Settings.Default.LoginMusicVolume / 100;
                }
                Sound.IsChecked = false;
            }
            else
                Sound.IsChecked = true;

            if (DateTime.Now.Month == 4 && DateTime.Now.Day == 1)
            {
                string SkinPath = Path.Combine(Client.ExecutingDirectory, "Assets", "champions",
                    "Teemo_Splash_" + new Random().Next(0, 8) + ".jpg");
                if (File.Exists(SkinPath))
                {
                    LoginImage.Source = new BitmapImage(new System.Uri(SkinPath, UriKind.Absolute));
                }
            }
            else if (Settings.Default.LoginPageImage == "" && !String.IsNullOrEmpty(themeLocation))
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

            Client.SQLiteDatabase = new SQLiteConnection(Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite"));

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
                    MessageTextBox = { Text = "Database is broken. Click OK to exit Sightstone." },
                    MessageTitle = { Content = "Database Error" }
                };
                Client.SQLiteDatabase.Close();
                File.Delete(Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite"));
                overlay.AcceptButton.Click += (o, i) => { Environment.Exit(0); };
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
                return;
            }

            var FreeToPlay = FreeToPlayChampions.GetInstance();


            Client.ChampionSkins = (from s in Client.SQLiteDatabase.Table<championSkins>()
                                    orderby s.name
                                    select s).ToList();
            Client.ChampionAbilities = (from s in Client.SQLiteDatabase.Table<championAbilities>()
                                        orderby s.name
                                        select s).ToList();

            foreach (var champ in Client.Champions)
            {
                var source = new Uri(Path.Combine(Client.ExecutingDirectory, "Assets", "champions", champ.iconPath),
                    UriKind.Absolute);
                if (File.Exists(source.AbsolutePath))
                    champ.icon = new BitmapImage(source);

                if (FreeToPlay != null)
                {
                    champ.IsFreeToPlay = FreeToPlay.IsFreeToPlay(champ);
                }

                var c1 = champ;
                foreach (var skins in Client.ChampionSkins.Where(skins => c1.id == skins.championId))
                {
                    var skin = new Skins
                    {
                        Id = skins.id,
                        Name = skins.displayName,
                        Num = skins.rank,
                        PortraitPath = skins.portraitPath,
                        IsBase = false,
                        SplashPath = skins.splashPath
                    };
                    if (skins.isBase == 1)
                    {
                        skin.IsBase = true;
                    }
                    if (string.IsNullOrWhiteSpace(skin.Name))
                        skin.Name = champ.displayName;
                    champ.Skins.Add(skin);
                }

                foreach (var spell in Client.ChampionAbilities)
                {
                    if (spell.championId == champ.id)
                    {
                        var newSpell = new Spell();
                        newSpell.Description = spell.description;
                        newSpell.Tooltip = spell.effect;
                        newSpell.ID = champ.displayName + spell.hotkey;
                    }
                }

                //Champions.InsertExtraChampData(champ);
            }

            Client.SQLiteDatabase.Close();

            Client.Masteries = Masteries.PopulateMasteries(new WebClient().DownloadString("http://ddragon.leagueoflegends.com/cdn/5.19.1/data/en_US/mastery.json"));
            Client.Runes = Runes.PopulateRunes(new WebClient().DownloadString("http://ddragon.leagueoflegends.com/cdn/5.19.1/data/en_US/rune.json"));
            //BaseUpdateRegion updateRegion = BaseUpdateRegion.GetUpdateRegion(user.Instance.UpdateRegion);
            BaseUpdateRegion updateRegion = BaseUpdateRegion.GetUpdateRegion("Live");
            var patcher = new RiotPatcher();
            //if (user.Instance.UpdateRegion != "Garena")
            if ("Live" != "Garena")
            {
                string tempString = patcher.GetListing(updateRegion.AirListing);

                string[] packages = patcher.GetManifest(
                    updateRegion.AirManifest + "releases/" + tempString + "/packages/files/packagemanifest");
                foreach (
                    string usestring in
                        packages.Select(package => package.Split(',')[0])
                            .Where(usestring => usestring.Contains("ClientLibCommon.dat")))
                    new WebClient().DownloadFile(new System.Uri(updateRegion.BaseLink + usestring),
                        Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"));
            }
            var reader = new SWFReader(Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"));
            foreach (var secondSplit in from abcTag in reader.Tags.OfType<DoABC>()
                                        where abcTag.Name.Contains("riotgames/platform/gameclient/application/Version")
                                        select Encoding.Default.GetString(abcTag.ABCData)
                                            into str
                                        select str.Split((char)6)
                                                into firstSplit

                                        select firstSplit[0].Split((char)18))


                if (secondSplit.Count() > 1)
                {
                    Client.Version = secondSplit[1];
                }
                else
                {
                    var thirdSplit = secondSplit[0].Split((char)19);
                    Client.Version = thirdSplit[1];
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

        private string GetLolRootPath(bool restart)
        {
            if (!restart)
            {
                switch (RegionComboBox.SelectedValuePath)
                {
                    case "PBE":
                        if (Settings.Default.PBELocation != string.Empty)
                            return Settings.Default.PBELocation;
                        break;
                    case "Live":
                        if (Settings.Default.LiveLocation != string.Empty)
                            return Settings.Default.LiveLocation;
                        break;
                    case "Korea":
                        if (Settings.Default.KRLocation != string.Empty)
                            return Settings.Default.KRLocation;
                        break;
                    case "Garena":
                        if (Settings.Default.GarenaLocation != string.Empty && Settings.Default.GarenaLocation.EndsWith("lol.exe"))
                        {
                            Settings.Default.GarenaLocation = Settings.Default.GarenaLocation.Replace("lol.exe", "");
                            return Settings.Default.GarenaLocation;
                        }
                        if (Settings.Default.GarenaLocation != string.Empty)
                            return Settings.Default.GarenaLocation;
                        break;
                }
                var possiblePaths = new List<Tuple<string, string>>
                {
                    new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\SightstoneLol",
                        "Path"),
                    new Tuple<string, string>(
                        @"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\RIOT GAMES", "Path"),
                    new Tuple<string, string>(
                        @"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\Wow6432Node\RIOT GAMES",
                        "Path"),
                    new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\RIOT GAMES", "Path"),
                    new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\Wow6432Node\Riot Games", "Path"),
                    new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Riot Games\League Of Legends", "Path"),
                    new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games", "Path"),
                    new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games\League Of Legends",
                        "Path"),
                    // Yes, a f*ckin whitespace after "Riot Games"..
                    new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games \League Of Legends",
                        "Path"),
                };
                foreach (var tuple in possiblePaths)
                {
                    var path = tuple.Item1;
                    var valueName = tuple.Item2;
                    try
                    {
                        var value = Registry.GetValue(path, valueName, string.Empty);
                        if (value == null || value.ToString() == string.Empty) continue;
                        var regKey = Registry.CurrentUser.CreateSubKey("Sightstone");
                        if (regKey == null) return value.ToString();
                        regKey.SetValue(
                            value.ToString().Contains("lol.exe") ? "GarenaLocation" : "LoLLocation",
                            value.ToString());
                        regKey.Close();
                        return value.ToString();
                    }
                    catch (Exception e)
                    {
                        Client.Log(e);
                    }
                }
            }

            var findLeagueDialog = new OpenFileDialog();

            if (!Directory.Exists(Path.Combine("C:\\", "Riot Games", "League of Legends")))
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Program Files (x86)", "GarenaLoL", "GameData",
                    "Apps", "LoL");
            else
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Riot Games", "League of Legends");

            findLeagueDialog.DefaultExt = ".exe";
            findLeagueDialog.Filter = "League of Legends Launcher|lol.launcher*.exe|Garena Launcher|lol.exe";

            var result = findLeagueDialog.ShowDialog();
            if (result != true)
                return string.Empty;

            var key = Registry.CurrentUser.CreateSubKey("Software\\RIOT GAMES");
            key?.SetValue("Path",
                findLeagueDialog.FileName.Replace("lol.launcher.exe", string.Empty).Replace("lol.launcher.admin.exe", string.Empty));

            if (restart)
            {
                MessageBox.Show("Saved value, please restart the client to login.", "press any key to restart");
                Process.Start(Path.Combine(Client.ExecutingDirectory, "Client", "Sightstone.exe"));
            }

            return findLeagueDialog.FileName.Replace("lol.launcher.exe", string.Empty).Replace("lol.launcher.admin.exe", string.Empty);
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
                    LoginPic.Source = new System.Uri("http://eddy5641.github.io/Sightstone/Login/Login.png");
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

        private void ShowLogin_Click(object sender, RoutedEventArgs e)
        {
            if (LoginGrid.IsVisible)
            {
                LoginGrid.Visibility = Visibility.Hidden;
            }
            else
            {
                LoginGrid.Visibility = Visibility.Visible;
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
                await garenaLogin(BaseRegion.GetRegion((string)RegionComboBox.SelectedValue), new UserClient { Garena = true });
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

            EncryptBox.Visibility = Visibility.Hidden;
            HideGrid.Visibility = Visibility.Hidden;
            ErrorTextBox.Visibility = Visibility.Hidden;
            LoggingInLabel.Visibility = Visibility.Visible;
            LoggingInProgressRing.Visibility = Visibility.Visible;
            BaseRegion selectedRegion = BaseRegion.GetRegion((string)RegionComboBox.SelectedValue);

            Login(LoginUsernameBox.Text, LoginPasswordBox.Password, selectedRegion, new UserClient());
            if (sender != null)
                switchpage = true;
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
        async void Login(string username, string pass, BaseRegion selectedRegion, Deletable<UserClient> user)
        {
            user.Instance.Region = selectedRegion;
            user.Instance.calls = new RiotCalls(user.Instance);
            //BaseRegion selectedRegion = BaseRegion.GetRegion((string)RegionComboBox.SelectedValue);
            var authToken = await user.Instance.calls.GetRestToken(username, pass, selectedRegion.LoginQueue);
            if (authToken == "invalid_credentials")
            {
                ErrorTextBox.Text = "Wrong login data";
                user.Delete();
                HideGrid.Visibility = Visibility.Visible;
                ErrorTextBox.Visibility = Visibility.Visible;
                LoggingInLabel.Visibility = Visibility.Hidden;
                LoggingInProgressRing.Visibility = Visibility.Collapsed;
                return;
            }
            user.Instance.RiotConnection = new RtmpClient(new System.Uri("rtmps://" + selectedRegion.Server + ":2099"), user.Instance.calls.RegisterObjects(), ObjectEncoding.Amf3);
            user.Instance.RiotConnection.CallbackException += client_CallbackException;
            user.Instance.RiotConnection.MessageReceived += client_MessageReceived;
            try
            {

                await user.Instance.RiotConnection.ConnectAsync();
            }
            catch
            {
                ErrorTextBox.Text = "Failed to login. SSL error.";
                HideGrid.Visibility = Visibility.Visible;
                ErrorTextBox.Visibility = Visibility.Visible;
                LoggingInLabel.Visibility = Visibility.Hidden;
                LoggingInProgressRing.Visibility = Visibility.Collapsed;
                //user.Delete();
                Login(username, pass, selectedRegion, user);
                return;
            }

            user.Instance.RiotConnection.SetChunkSize(2147483647);
            AuthenticationCredentials newCredentials = new AuthenticationCredentials
            {
                Username = username,
                Password = pass,
                ClientVersion = Client.Version,
                IpAddress = user.Instance.calls.GetIpAddress(),
                Locale = selectedRegion.Locale,
                OperatingSystem = "Windows 7",
                Domain = "lolclient.lol.riotgames.com",
                AuthToken = authToken
            };
            Session login = await user.Instance.calls.Login(newCredentials);
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
            user.Instance.PlayerSession = login;

            var str1 = string.Format("gn-{0}", login.AccountSummary.AccountId);
            var str2 = string.Format("cn-{0}", login.AccountSummary.AccountId);
            var str3 = string.Format("bc-{0}", login.AccountSummary.AccountId);
            Task<bool>[] taskArray = { user.Instance.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str1, str1),
                                                 user.Instance.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str2, str2),
                                                 user.Instance.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", "bc", str3) };

            await Task.WhenAll(taskArray);
            //Riot added this for no reason but make it look like the riot client we have to do this
            var plainTextbytes = Encoding.UTF8.GetBytes(login.AccountSummary.Username + ":" + login.Token);
            user.Instance.reconnectToken = Convert.ToBase64String(plainTextbytes);

            var LoggedIn = await user.Instance.RiotConnection.LoginAsync(username.ToLower(), login.Token);

            var packetx = await user.Instance.calls.GetLoginDataPacketForUser();

            if (packetx == null || packetx.AllSummonerData == null)
            {
                user.Instance.RiotConnection.CallbackException -= client_CallbackException;
                user.Instance.RiotConnection.MessageReceived -= client_MessageReceived;
                //Just Created Account, need to put logic here.
                user.Instance.done = false;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var createSummoner = new CreateSummonerNameOverlay(user.Instance);
                    Client.OverlayContainer.Content = createSummoner.Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                while (!user.Instance.done)
                {
                }
                Login(username, pass, selectedRegion, user);
                return;
            }

            if (saveuser || !string.IsNullOrWhiteSpace(Client.EncrytKey))
            {
                UserList.AddUser(username, pass, packetx.AllSummonerData.Summoner.InternalName,
                                    "Using Sightstone", packetx.AllSummonerData.Summoner.ProfileIconId,
                                    selectedRegion, ShowType.chat, Client.EncrytKey);
                saveuser = false;
            }
            DoGetOnLoginPacket(username, pass, selectedRegion, packetx, user);
        }

        private async void DoGetOnLoginPacket(string username, string pass, BaseRegion selectedRegion, LoginDataPacket packetx, Deletable<UserClient> user)
        {
            //TODO: Finish this so all calls are used

            user.Instance.Queues = await user.Instance.calls.GetAvailableQueues();
            user.Instance.PlayerChampions = await user.Instance.calls.GetAvailableChampions();
            //var runes = await RiotCalls.GetSummonerRuneInventory(packetx.AllSummonerData.Summoner.AcctId);
            user.Instance.StartHeartbeat();
            //var leaguePos = await RiotCalls.GetMyLeaguePositions();
            //var preferences = await RiotCalls.LoadPreferencesByKey();
            //var masterybook = await RiotCalls.GetMasteryBook(packetx.AllSummonerData.Summoner.AcctId);
            //var lobby = await RiotCalls.CheckLobbyStatus();
            var invites = await user.Instance.calls.GetPendingInvitations();
            //var player = await RiotCalls.CreatePlayer();
            GotLoginPacket(packetx, username, pass, selectedRegion, user);

            foreach (var pop in from InvitationRequest invite in invites
                                select new GameInvitePopup(invite, user.Instance)
                                {
                                    HorizontalAlignment = HorizontalAlignment.Right,
                                    VerticalAlignment = VerticalAlignment.Bottom,
                                    Height = 230
                                })
                Client.NotificationGrid.Children.Add(pop);
            if (invites.Length != 0)
                Client.MainWin.FlashWindow();

            user.Instance.LoginPacket = packetx;
            if (!dataLogin.ContainsKey(packetx.AllSummonerData.Summoner.InternalName + ":" + selectedRegion))
            {
                dataLogin.Add(packetx.AllSummonerData.Summoner.InternalName + ":" + selectedRegion, new LoginData
                {
                    Pass = pass,
                    Region = selectedRegion,
                    ShowType = ShowType.chat,
                    Status = "Using Sightstone",
                    SumIcon = packetx.AllSummonerData.Summoner.ProfileIconId,
                    SumName = packetx.AllSummonerData.Summoner.InternalName,
                    User = username
                });

                LoggingInLabel.Visibility = Visibility.Hidden;
                LoggingInProgressRing.Visibility = Visibility.Hidden;
                Encrypt.Visibility = Visibility.Hidden;
                EncryptCheck.Visibility = Visibility.Hidden;
            }
        }

        private void GotLoginPacket(LoginDataPacket packet, string username, string pass, BaseRegion selectedRegion, Deletable<UserClient> user)
        {

            user.Instance.LoginPacket = packet;
            Client.CurrentServer = selectedRegion.Server + ":" + packet.AllSummonerData.Summoner.InternalName;
            UserList.Users.Add(selectedRegion.Server + ":" + packet.AllSummonerData.Summoner.InternalName, new Dictionary<string, UserClient> { { packet.AllSummonerData.Summoner.InternalName, user.Instance } });
            if (packet.AllSummonerData.Summoner.ProfileIconId == -1)
            {
                user.Instance.RiotConnection.CallbackException -= client_CallbackException;
                user.Instance.RiotConnection.MessageReceived -= client_MessageReceived;
                user.Instance.done = false;
                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    Client.OverlayContainer.Content = new ChooseProfilePicturePage().Content;
                    Client.OverlayContainer.Visibility = Visibility.Visible;
                }));
                while (!user.Instance.done)
                {
                }
                Login(username, pass, selectedRegion, user);

                return;
            }

            user.Instance.RiotConnection.MessageReceived += user.Instance.OnMessageReceived;
            user.Instance.RiotConnection.Disconnected += user.Instance.RiotConnection_Disconnected;
            user.Instance.GameConfigs = packet.GameTypeConfigs;
            user.Instance.IsLoggedIn = true;


            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(async () =>
            {
                //MessageBox.Show("Do not play ANY games. I am not sure if they will work ~eddy", "XMPP", MessageBoxButton.OK, MessageBoxImage.Warning);
                Client.StatusContainer.Visibility = Visibility.Visible;
                Client.Container.Margin = new Thickness(0, 0, 0, 40);
                user.Instance.XmppConnection = new agsXMPP.XmppClientConnection("pvp.net", 5223)
                {
                    AutoResolveConnectServer = false,
                    ConnectServer = "chat." + user.Instance.Region.ChatName + ".lol.riotgames.com",
                    Resource = "xiff",
                    UseSSL = true,
                    KeepAliveInterval = 10,
                    KeepAlive = true,
                    UseCompression = true
                };
                user.Instance.XmppConnection.OnMessage += user.Instance.XmppConnection_OnMessage;
                user.Instance.XmppConnection.OnError += user.Instance.XmppConnection_OnError;
                if (switchpage)
                {
                    user.Instance.XmppConnection.OnLogin += (o) =>
                    {
                        Client.Log("Connected to XMPP Server");
                        //Set up chat
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            user.Instance.XmppConnection.Send(invisibleLoginCheckBox.IsChecked != true
                                ? new Presence(ShowType.chat, user.Instance.GetPresence(), 0)
                                {
                                    Type = PresenceType.available
                                }
                                : new Presence(ShowType.NONE, user.Instance.GetPresence(), 0)
                                {
                                    Type = PresenceType.invisible
                                });
                        }));
                    };
                }
                Client.ready = false;
                Client.loadedGroups = false;
                user.Instance.RostManager = new RosterManager(user.Instance.XmppConnection);
                user.Instance.XmppConnection.OnRosterItem += user.Instance.RostManager_OnRosterItem;

                user.Instance.XmppConnection.OnRosterEnd += user.Instance.XmppConnection_OnRosterEnd;
                user.Instance.PresManager = new PresenceManager(user.Instance.XmppConnection);
                user.Instance.XmppConnection.OnPresence += user.Instance.XmppConnection_OnPresence;
                user.Instance.XmppConnection.OnMessage += Client.statusPage.XmppConnection_OnMessage;
                if (!user.Instance.Garena)
                {
                    user.Instance.userpass = new KeyValuePair<string, string>(username,
                        "AIR_" + pass);

                    user.Instance.XmppConnection.Open(username, "AIR_" + pass);

                    //Client.XmppConnection.OnInvalidCertificate += Client.XmppConnection_OnInvalidCertificate;
                }
                else
                {
                    user.Instance.XmppConnection.ConnectServer = "chat" + user.Instance.Region.ChatName + ".lol.garenanow.com";
                    var gas = getGas(user);
                    user.Instance.XmppConnection.Open(user.Instance.UID, "AIR_" + gas);
                    user.Instance.userpass = new KeyValuePair<string, string>(user.Instance.UID, "AIR_" + gas);
                }

                Client.Log("Connected to " + user.Instance.Region.RegionName + " and logged in as " +
                           user.Instance.LoginPacket.AllSummonerData.Summoner.Name);

                PlatformGameLifecycleDTO data = (PlatformGameLifecycleDTO)user.Instance.LoginPacket.ReconnectInfo;
                Client.CurrentUser = packet.AllSummonerData.Summoner.InternalName;
                Client.MainPage = new MainPage();

                if (data != null && data.Game != null)
                {
                    Client.Log(data.PlayerCredentials.ChampionId.ToString(CultureInfo.InvariantCulture));
                    user.Instance.CurrentGame = data.PlayerCredentials;
                    user.Instance.GameType = data.Game.GameType;
                    if (switchpage)
                        Client.SwitchPage(new InGame());
                }
                else if (switchpage)
                {
                    while (!Client.ready)
                    {
                        await Task.Delay(1000);
                    }
                    Client.SwitchPage(Client.MainPage);

                    Client.loadedGroups = true;
                }
                if (switchpage) return;
                var sum = dataLogin[packet.AllSummonerData.Summoner.InternalName + ":" + selectedRegion];
                user.Instance.presenceStatus = sum.ShowType;
                user.Instance.XmppConnection.Send(sum.ShowType == ShowType.NONE
                    ? new Presence(sum.ShowType, user.Instance.GetPresence(), 0) { Type = PresenceType.invisible }
                    : new Presence(sum.ShowType, user.Instance.GetPresence(), 0) { Type = PresenceType.available });
                UserAccount acc = new UserAccount
                {
                    PlayerName = { Content = packet.AllSummonerData.Summoner.InternalName },
                    StatusColour = { Fill = System.Windows.Media.Brushes.Green },
                    ProfileImage =
                    {
                        Source = new BitmapImage(Client.GetIconUri(sum.SumIcon))
                    },
                    RegionLabel = { Content = sum.Region.RegionName },
                    LevelLabel = { Content = packet.AllSummonerData.SummonerLevel.Level },
                    PlayerStatus = { Content = sum.Status }
                };
                foreach (var lgndata in dataLogin.Where(lgndata => lgndata.Key == packet.AllSummonerData.Summoner.InternalName + ":" + selectedRegion))
                {
                    user.Instance.presenceStatus = lgndata.Value.ShowType;
                    user.Instance.XmppConnection.Send(lgndata.Value.ShowType == ShowType.NONE
                        ? new Presence(sum.ShowType, user.Instance.GetPresence(), 0) { Type = PresenceType.invisible }
                        : new Presence(sum.ShowType, user.Instance.GetPresence(), 0) { Type = PresenceType.available });
                }
                switch (user.Instance.presenceStatus)
                {
                    case ShowType.away:
                    case ShowType.dnd:
                    case ShowType.xa:
                        acc.StatusColour.Fill = System.Windows.Media.Brushes.Red;
                        break;
                    case ShowType.NONE:
                        acc.StatusColour.Fill = System.Windows.Media.Brushes.Silver;
                        break;
                }

                user.Instance.userAccount = acc;
                UserListView.Items.Add(acc);
                acc.MouseDown += async (o, e) =>
                {
                    Client.CurrentServer = selectedRegion.Server + ":" + packet.AllSummonerData.Summoner.InternalName;
                    Client.CurrentUser = packet.AllSummonerData.Summoner.InternalName;
                    Client.SwitchPage(Client.MainPage);
                    while (!Client.ready)
                    {
                        await Task.Delay(1000);
                    }
                    Client.loadedGroups = true;
                };
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
        private double lastVolume;

        private async Task garenaLogin(BaseRegion garenaregion, Deletable<UserClient> user)
        {
            user.Instance.Region = garenaregion;
            user.Instance.calls = new RiotCalls(user.Instance);
            WindowsIdentity winIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal winPrincipal = new WindowsPrincipal(winIdentity);
            if (!winPrincipal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                var overlay = new MessageOverlay
                {
                    MessageTextBox =
                    {
                        Text = "Press OK to restart as admin."
                    },
                    MessageTitle = { Content = "Insufficient Privledges" }
                };
                Client.OverlayContainer.Content = overlay.Content;
                Client.OverlayContainer.Visibility = Visibility.Visible;
                overlay.AcceptButton.Click += (o, i) =>
                {
                    var info = new ProcessStartInfo(Path.Combine(Client.ExecutingDirectory, "Client", "Sightstone.exe"))
                    {
                        UseShellExecute = true,
                        Verb = "runas"
                    };
                    try
                    {
                        Process.Start(info);
                    }
                    catch (Win32Exception e) //User click "No" in UAC dialog or user does not have sufficient permission
                    {
                        overlay = new MessageOverlay
                        {
                            MessageTextBox =
                            {
                                Text = "Windows UAC denied Sightstone from restarting as admin."
                            },
                            MessageTitle = { Content = "Insufficient Permission" }
                        };
                        Client.OverlayContainer.Content = overlay.Content;
                        Client.OverlayContainer.Visibility = Visibility.Visible;
                    }
                    Environment.Exit(0);
                };
                return;
            }

            LoggingInLabel.Content = "Waiting for user to launch League from garena";
            HideGrid.Visibility = Visibility.Hidden;
            ErrorTextBox.Visibility = Visibility.Hidden;
            LoggingInLabel.Visibility = Visibility.Visible;
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
                            s1 = s1.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries)[1];
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
                    shouldExit = true;
                    user.Instance.Region = garenaregion;
                    Dispatcher.BeginInvoke(
                        DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                HideGrid.Visibility = Visibility.Hidden;
                                ErrorTextBox.Visibility = Visibility.Hidden;
                                LoggingInLabel.Visibility = Visibility.Visible;
                                LoggingInLabel.Content = "Logging in...";
                                LoggingInProgressRing.Visibility = Visibility.Visible;
                            }));
                    var context = user.Instance.calls.RegisterObjects();
                    user.Instance.RiotConnection = new RtmpClient(new System.Uri("rtmps://" + garenaregion.Server + ":2099"), context, ObjectEncoding.Amf3);
                    user.Instance.RiotConnection.CallbackException += client_CallbackException;
                    user.Instance.RiotConnection.MessageReceived += client_MessageReceived;
                    await user.Instance.RiotConnection.ConnectAsync();
                    user.Instance.RiotConnection.SetChunkSize(2147483647);

                    AuthenticationCredentials newCredentials = new AuthenticationCredentials
                    {
                        AuthToken = await
                            user.Instance.calls.GetRestToken(LoginUsernameBox.Text, LoginPasswordBox.Password, garenaregion.LoginQueue, reToken(s1)),
                        Username = user.Instance.UID,
                        Password = null,
                        ClientVersion = Client.Version,
                        IpAddress = user.Instance.calls.GetIpAddress(),
                        Locale = garenaregion.Locale,
                        PartnerCredentials = "8393 " + s1,
                        OperatingSystem = "Windows 7",
                        Domain = "lolclient.lol.riotgames.com",
                    };
                    Session login = await user.Instance.calls.Login(newCredentials);
                    user.Instance.PlayerSession = login;
                    var str1 = string.Format("gn-{0}", login.AccountSummary.AccountId);
                    var str2 = string.Format("cn-{0}", login.AccountSummary.AccountId);
                    var str3 = string.Format("bc-{0}", login.AccountSummary.AccountId);
                    Task<bool>[] taskArray = { user.Instance.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str1, str1),
                                                 user.Instance.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", str2, str2),
                                                 user.Instance.RiotConnection.SubscribeAsync("my-rtmps", "messagingDestination", "bc", str3) };

                    await Task.WhenAll(taskArray);
                    var LoggedIn = await user.Instance.RiotConnection.LoginAsync(user.Instance.UID, login.Token);
                    var packet = await user.Instance.calls.GetLoginDataPacketForUser();
                    DoGetOnLoginPacket(user.Instance.UID, null, garenaregion, packet, user);
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
                Settings.Default.Save();

                //user.Instance.UpdateRegion = (string)UpdateRegionComboBox.SelectedValue;
                /*
                if (!RegionComboBox.Items.IsInUse)
                {
                    RegionComboBox.Items.Clear();
                    Thread.Sleep(100);
                }
                //*/
                switch ((string)UpdateRegionComboBox.SelectedValue)
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

        public string getGas(Deletable<UserClient> user)
        {
            //string begin = "{\"signature\":\"";
            //string end = "}";
            string gas = user.Instance.Gas;
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
            if ((AutoLoginCheckBox.IsChecked == true && RememberUsernameCheckbox.IsChecked == true && RememberPasswordCheckbox.IsChecked == true) || (string)UpdateRegionComboBox.SelectedValue == "Garena")
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
            Dispatcher.BeginInvoke(new ThreadStart(() =>
            {
                AutoLoginCheckBox.IsChecked = Settings.Default.AutoLogin;
            }), DispatcherPriority.Input);

            System.Timers.Timer timer = new System.Timers.Timer { Interval = 3000 };

            timer.Elapsed += (s, a) =>
            {
                timer.Stop();

                bool autoLogin = false;
                Dispatcher.Invoke(() =>
                {
                    autoLogin = (bool)AutoLoginCheckBox.IsChecked;
                });

                if (autoLogin)
                {
                    Client.Log("Auto login");
                    Dispatcher.BeginInvoke(new ThreadStart(() =>
                    {
                        LoginButton_Click(1, null);
                    }), DispatcherPriority.Input);
                }
            };
            //timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Encrypt.Password))
            {
                MessageBox.Show("Please enter an encryption password");
                return;
            }
            if (!UserList.VerifyEncrypt(Encrypt.Password))
            {
                MessageBox.Show("Encryption is WRONG");
                return;
            }
            Client.EncrytKey = Encrypt.Password;
            authed = true;
            List<LoginData> data = UserList.GetAllUsers(Encrypt.Password);
            foreach (var acc in data)
            {
                Client.Log("Found " + acc.SumName);
#if DEBUG
                Client.Log(string.Format("User: {0} Pass: {1} Region: {2}", acc.User, acc.Pass, acc.Region));
#endif
                dataLogin.Add(acc.SumName + ":" + acc.Region, acc);
                if (acc.Region.Garena)
                    garenaLogin(acc.Region, new UserClient { Garena = true });
                else
                    Login(acc.User, acc.Pass, acc.Region, new UserClient());
            }
            EncryptBox.Visibility = Visibility.Hidden;
        }

        private void AddAccountButton_Click(object sender, RoutedEventArgs e)
        {
            Client.Log("starting to login");
            if (!authed)
            {
                ErrorTextBox.Text = "You must auth yourself first";
                Client.Log("Auth first");
                return;
            }

            var region = BaseRegion.GetRegion((string)RegionComboBox.SelectedValue);
            saveuser = true;
            if (region.Garena)
                garenaLogin(region, new UserClient());
            else
                Login(LoginUsernameBox.Text, LoginPasswordBox.Password, region, new UserClient());
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if ((sender as Slider).IsInitialized)
            {
                SoundPlayer.Volume = e.NewValue / 100;
                Settings.Default.LoginMusicVolume = e.NewValue;
                Settings.Default.Save();
                lastVolume = e.OldValue / 100;
                if (e.NewValue == 0)
                {
                    //Change to muted icon
                    muteButton.Data = Geometry.Parse("M16.5 12c0-1.77-1.02-3.29-2.5-4.03v2.21l2.45 2.45c.03-.2.05-.41.05-.63zm2.5 0c0 .94-.2 1.82-.54 2.64l1.51 1.51C20.63 14.91 21 13.5 21 12c0-4.28-2.99-7.86-7-8.77v2.06c2.89.86 5 3.54 5 6.71zM4.27 3L3 4.27 7.73 9H3v6h4l5 5v-6.73l4.25 4.25c-.67.52-1.42.93-2.25 1.18v2.06c1.38-.31 2.63-.95 3.69-1.81L19.73 21 21 19.73l-9-9L4.27 3zM12 4L9.91 6.09 12 8.18V4z");
                }
                else
                {
                    //Change to volume icon
                    muteButton.Data = Geometry.Parse("M3 9v6h4l5 5V4L7 9H3zm13.5 3c0-1.77-1.02-3.29-2.5-4.03v8.05c1.48-.73 2.5-2.25 2.5-4.02zM14 3.23v2.06c2.89.86 5 3.54 5 6.71s-2.11 5.85-5 6.71v2.06c4.01-.91 7-4.49 7-8.77s-2.99-7.86-7-8.77z");
                }
            }
        }

        private void MuteButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).IsInitialized)
            {
                if (SoundPlayer.Volume == 0)
                {
                    if (lastVolume != 0)
                    {
                        SoundPlayer.Volume = lastVolume;
                        slider.Value = lastVolume * 100;
                    }
                    else
                    {
                        SoundPlayer.Volume = 0.5;
                        slider.Value = 50;
                    }
                }
                else
                {
                    slider.Value = 0;
                }
            }
        }
    }
}
