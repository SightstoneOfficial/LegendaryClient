using System.Collections.Generic;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using LegendaryClient.Logic;
using LegendaryClient.Logic.JSON;
using LegendaryClient.Logic.Patcher;
using LegendaryClient.Logic.SQLite;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Linq;
using RAFlibPlus;
using ComponentAce.Compression.Libs.zlib;

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for PatcherPage.xaml
    /// </summary>
    public partial class PatcherPage : Page
    {
        internal static bool LoLDataIsUpToDate = false;
        internal static string LatestLolDataVersion = "";
        internal static string LolDataVersion = "";
        public PatcherPage()
        {
            InitializeComponent();
            StartPatcher();
            Client.Log("LegendaryClient Started Up Successfully");
            DevKey.TextChanged += DevKeySend_Click;
        }
        #region Devkeys
        private void DevKeySend_Click(object sender, RoutedEventArgs e)
        {
            if (DevKey.Text == "!~devkey-publicdev~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Public Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-R4tP769DmAr2B3xHeRvm~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Eddy5641!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-AGWV2t7GknpBDzzTeyze~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome ICareForCows!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-uXaRQLB2h82EZqeFwMS9~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dave!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-xnfRtZUr5WzYMUHmYLHs~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Rftiiv15!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-9mnDreEAPtUeCwAVkjf3~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-UwYtjHckU3TmnGbAGb7w~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-Bc3qNxzj69ECv3YKpLwf~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-YHR5zaUYMWE22LLUCrRf~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-qQCrKBbmkA4sVdwR76sE~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
            else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                else if (DevKey.Text == "!~betakey-gBEtVpJgj6QpSWZzHVRN~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Beta User!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                    else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }
                        else if (DevKey.Text == "!~devkey-8ZTKHEumL8EUXvLYt92n~!")
            {
                DevKey.Visibility = Visibility.Hidden;
                Welcome.Text = "Welcome Dev!";
                DevKeySend.Visibility = Visibility.Hidden;
                DevSkip.Visibility = Visibility.Visible;
                Welcome.Visibility = Visibility.Visible;
                Client.Dev = true;
                Client.NewStatus();
            }

            else if (Client.Dev == true)
            {
                Client.Log("Dev mode enabled");
            }
        #endregion Devkeys
        }

        private void DevSkip_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new LoginPage());
            Client.Log("Swiched to LoginPage with DevSkip");
        }

        private void SkipPatchButton_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new LoginPage());
        }

        private void StartPatcher()
        {
            Thread bgThead = new Thread(() =>
            {
                LogTextBox("Starting Patcher");

                WebClient client = new WebClient();
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadDDragon);
                client.DownloadProgressChanged += (o, e) =>
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        double bytesIn = double.Parse(e.BytesReceived.ToString());
                        double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                        double percentage = bytesIn / totalBytes * 100;
                        CurrentProgressLabel.Content = "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                        CurrentProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
                    }));
                };

                #region LegendaryClient

                string CurrentMD5 = GetMd5();
                LogTextBox("MD5: " + CurrentMD5);
                string VersionString = "";


                UpdateData legendaryupdatedata = new UpdateData();
                var version = LegendaryClientPatcher.GetLatestLCVersion();
                LogTextBox("Most Up to date LegendaryClient Version: " + version);
                string versionAsString = version;

                if (version != Client.LegendaryClientVersion)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        MessageOverlay overlay = new MessageOverlay();
                        overlay.MessageTextBox.Text = "An update is available LegendaryClient";
                        overlay.MessageTitle.Content = "Update Notification";
                        overlay.AcceptButton.Content = "Update LegendaryClient";
                        overlay.AcceptButton.Click += update;
                        overlay.MessageTextBox.TextChanged += Text_Changed;
                        Client.OverlayContainer.Content = overlay.Content;
                        //Client.OverlayContainer.Visibility = Visibility.Visible;

                        CurrentProgressLabel.Content = "LegendaryClient Is Out of Date!";
                        
                    }));
                    LogTextBox("LegendaryClient Is Out of Date!");

                    //return;
                }
                else if (Client.LegendaryClientVersion == version)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "LegendaryClient Is Up To Date!";
                        
                    }));
                    LogTextBox("LegendaryClient Is Up To Date!");
                }
                else if (version == null)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Could not check LegendaryClient Version!";
                        
                    }));
                    LogTextBox("Could not check LegendaryClient Version!");
                    return;
                }

                #endregion LegendaryClient

                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    TotalProgressLabel.Content = "20%";
                    TotalProgessBar.Value = 20;
                }));

                #region DDragon

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                if (!Directory.Exists("Assets"))
                {
                    Directory.CreateDirectory("Assets");
                }
                if (!File.Exists(Path.Combine("Assets", "VERSION_DDRagon")))
                {
                    var VersionLOL = File.Create(Path.Combine("Assets", "VERSION_DDRagon"));
                    VersionLOL.Write(encoding.GetBytes("0.0.0"), 0, encoding.GetBytes("0.0.0").Length);
                    
                    VersionLOL.Close();
                }
                

                RiotPatcher patcher = new RiotPatcher();
                string DDragonDownloadURL = patcher.GetDragon();
                LogTextBox("DataDragon Version: " + patcher.DDragonVersion);
                string DDragonVersion = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_DDragon"));
                LogTextBox("Current DataDragon Version: " + DDragonVersion);

                Client.Version = DDragonVersion;
                Client.Log("DDragon Version (LOL Version) = " + DDragonVersion);

                 LogTextBox("Client Version: " + Client.Version);

                if (patcher.DDragonVersion != DDragonVersion)
                {
                    if (!Directory.Exists(Path.Combine("Assets", "temp")))
                    {
                        Directory.CreateDirectory(Path.Combine("Assets", "temp"));
                    }

                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Downloading DataDragon";
                    }));

                    client.DownloadFile(DDragonDownloadURL, Path.Combine("Assets", "dragontail-" + patcher.DDragonVersion + ".tgz"));

                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Extracting DataDragon";
                    }));

                    Stream inStream = File.OpenRead(Path.Combine("Assets", "dragontail-" + patcher.DDragonVersion + ".tgz"));
                    
                    using (GZipInputStream gzipStream = new GZipInputStream(inStream))
                    {
                        TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                        tarArchive.ExtractContents(Path.Combine("Assets", "temp"));
                        tarArchive.CloseArchive();
                    }
                    inStream.Close();

                    Copy(Path.Combine("Assets", "temp", patcher.DDragonVersion, "data"), Path.Combine("Assets", "data"));
                    Copy(Path.Combine("Assets", "temp", patcher.DDragonVersion, "img"), Path.Combine("Assets"));
                    DeleteDirectoryRecursive(Path.Combine("Assets", "temp"));

                    var VersionDDragon = File.Create(Path.Combine("Assets", "VERSION_DDRagon"));
                    VersionDDragon.Write(encoding.GetBytes(patcher.DDragonVersion), 0, encoding.GetBytes(patcher.DDragonVersion).Length);

                    Client.Version = DDragonVersion;
                    VersionDDragon.Close();
                }

                #endregion DDragon

                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    TotalProgressLabel.Content = "40%";
                    TotalProgessBar.Value = 40;
                }));

                // Try get LoL path from registry

                //A string that looks like C:\Riot Games\League of Legends\
                string lolRootPath = GetLolRootPath();

                #region lol_air_client

                if (!File.Exists(Path.Combine("Assets", "VERSION_AIR")))
                {
                    var VersionAIR = File.Create(Path.Combine("Assets", "VERSION_AIR"));
                    VersionAIR.Write(encoding.GetBytes("0.0.0.0"), 0, encoding.GetBytes("0.0.0.0").Length);
                    VersionAIR.Close();
                }

                string LatestAIR = patcher.GetLatestAir();
                LogTextBox("Air Assets Version: " + LatestAIR);
                string AirVersion = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                LogTextBox("Current Air Assets Version: " + AirVersion);
                bool RetrieveCurrentInstallation = false;
                string AirLocation = "";

                if (AirVersion == "0.0.0.0")
                {
                    LogTextBox("Checking for existing League of Legends Installation");
                    AirLocation = Path.Combine("League of Legends", "RADS", "projects", "lol_air_client", "releases");
                    var localAirLocation = Path.Combine("RADS", "projects", "lol_air_client", "releases");
                    if (Directory.Exists(AirLocation))
                    {
                        RetrieveCurrentInstallation = true;
                    }
                    else if (string.IsNullOrEmpty(lolRootPath) == false && Directory.Exists(Path.Combine(lolRootPath, localAirLocation)))
                    {
                        RetrieveCurrentInstallation = true;
                        AirLocation = Path.Combine(lolRootPath, localAirLocation);
                    }
                    else
                    {
                        LogTextBox("Unable to find existing League of Legends. Copy your League of Legends folder into + "
                            + Client.ExecutingDirectory
                            + " to make the patching process quicker");
                    }

                    if (RetrieveCurrentInstallation)
                    {
                        LogTextBox("Getting Air Assets from " + AirLocation);
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            CurrentProgressLabel.Content = "Copying Air Assets";
                        }));
                        AirVersion = patcher.GetCurrentAirInstall(AirLocation);
                        LogTextBox("Retrieved currently installed Air Assets");
                        LogTextBox("Current Air Assets Version: " + AirVersion);
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            TotalProgressLabel.Content = "60%";
                            TotalProgessBar.Value = 60;
                        }));
                    }
                }

                if (AirVersion != LatestAIR)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        SkipPatchButton.IsEnabled = true;
                        CurrentProgressLabel.Content = "Retrieving Air Assets";
                    }));
                }

                #endregion lol_air_client


                //string GameVersion = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));
                #region lol_game_client
                LogTextBox("Trying to detect League of Legends GameClient");
                LogTextBox("League of Legends is located at: " + lolRootPath);
                //RADS\solutions\lol_game_client_sln\releases
                var GameLocation = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln", "releases");

                string LolVersion2 = new WebClient().DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_NA");
                string LolVersion = new WebClient().DownloadString("http://l3cdn.riotgames.com/releases/live/solutions/lol_game_client_sln/releases/releaselisting_NA");
                string GameClientSln = LolVersion.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
                string GameClient = LolVersion2.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
                LogTextBox("Latest League of Legends GameClient: " + GameClientSln);
                LogTextBox("Checking if League of Legends is Up-To-Date");

                string LolLauncherVersion = new WebClient().DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_NA");
                string LauncherVersion = LolLauncherVersion.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
                if (Directory.Exists(Path.Combine(GameLocation, GameClientSln)))
                {
                    LogTextBox("League of Legends is Up-To-Date");
                    //Client.LaunchGameLocation = Path.Combine(Client.GameLocation, GameClientSln);
                    //C:\Riot Games\League of Legends\RADS\projects\lol_game_client\releases\0.0.0.243\deploy
                    Client.LOLCLIENTVERSION = LolVersion2;
                    //C:\Riot Games\League of Legends\RADS\solutions\lol_game_client_sln\releases\0.0.1.50\deploy
                    Client.Location = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln", "releases", GameClientSln, "deploy");
                    //C:\Riot Games\League of Legends\RADS\projects\lol_air_client\releases\0.0.1.104
                    Client.LoLLauncherLocation = Path.Combine(lolRootPath, "RADS", "projects", "lol_air_client", "releases", LauncherVersion, "deploy");
                    Client.RootLocation = lolRootPath;
                }
                else 
                {
                    LogTextBox("League of Legends is not Up-To-Date. Please Update League Of Legends");
                    return;
                }


                if (!Directory.Exists("RADS"))
                {
                    Directory.CreateDirectory("RADS");
                }

                if (!File.Exists(Path.Combine("RADS", "VERSION_LOL")))
                {
                    var VersionGAME = File.Create(Path.Combine("RADS", "VERSION_LOL"));
                    VersionGAME.Write(encoding.GetBytes("0.0.0.0"), 0, encoding.GetBytes("0.0.0.0").Length);
                    VersionGAME.Close();
                }

                string LatestGame = patcher.GetLatestGame();
                LogTextBox("League Of Legends Version: " + LatestGame);
                string GameVersion = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));
                LogTextBox("Current League of Legends Version: " + GameVersion);
                RetrieveCurrentInstallation = false;
                string NGameLocation = "";

                if (GameVersion != GameClient)
                {
                    LogTextBox("Checking for existing League of Legends Installation");
                    NGameLocation = Path.Combine("League of Legends", "RADS");
                    if (Directory.Exists(NGameLocation))
                    {
                        RetrieveCurrentInstallation = true;
                    }
                    else if (Directory.Exists(Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", NGameLocation)))
                    {
                        RetrieveCurrentInstallation = true;
                        NGameLocation = Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", NGameLocation);
                    }
                    else
                    {
                        LogTextBox("Unable to find existing League of Legends. Copy your League of Legends folder into + "
                            + Client.ExecutingDirectory
                            + " to make the patching process quicker");
                    }

                    if (RetrieveCurrentInstallation)
                    {
                        LogTextBox("Getting League Of Legends from " + NGameLocation);
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            CurrentProgressLabel.Content = "Copying League of Legends";
                        }));
                        GameVersion = patcher.GetCurrentGameInstall(NGameLocation);
                        LogTextBox("Retrieved currently installed League of Legends");
                        LogTextBox("Current League of Legends Version: " + NGameLocation);
                    }
                }

                if (GameVersion != LatestGame)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Retrieving League of Legends";
                    }));
                }
                //No Need to download this anymore, I will auto detect League of Legends
                /*
                if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client")))
                {
                    Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client"));
                }
                if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL")))
                {
                    var VersionLOL = File.Create(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));
                    VersionLOL.Write(encoding.GetBytes("0.0.0.0"),0,encoding.GetBytes("0.0.0.0").Length);
                    VersionLOL.Close();
                }
                LogTextBox("Checking version...");
                CheckIfPatched();
                if (!LoLDataIsUpToDate)
                {
                    LogTextBox("Not up-to-date!");
                    if (LolDataVersion == "0.0.0.0")
                    {
                        LogTextBox("Checking for existing LoL installation");
                        string FileArchivesDirectory = Path.Combine("League of Legends", "RADS", "projects", "lol_game_client", "filearchives");
                        string MainVersionLocation = Path.Combine("League of Legends", "RADS", "projects", "lol_game_client", "releases");
                        if (Directory.Exists(FileArchivesDirectory))
                        {
                            ExpandRAF(FileArchivesDirectory);
                            WriteLatestVersion(MainVersionLocation);
                        }
                        else if (Directory.Exists(Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", FileArchivesDirectory)))
                        {
                            ExpandRAF(Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", FileArchivesDirectory));
                            WriteLatestVersion(Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", MainVersionLocation));
                        }
                    }
                    string PackageManifest = "";
                    int CurrentVersionNumber = Convert.ToInt32(LolDataVersion.Split('.')[3]);
                    int LatestVersionNumber = Convert.ToInt32(LatestLolDataVersion.Split('.')[3]);
                    LogTextBox("Retrieving Package Manifest");
                    //How will this happen, idk but we will never know if it will happen
                    InvalidVersion:
                    if (CurrentVersionNumber >= LatestVersionNumber)
                    {
                        //Already updated, just fake numbers in the release listing and you can ignore them
                    }
                    try
                    {
                        PackageManifest = new WebClient().DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/0.0.0." + LatestVersionNumber + "/packages/files/packagemanifest");
                    }
                    catch { LogTextBox(LatestVersionNumber + " is not valid"); LatestVersionNumber -= 1; goto InvalidVersion; }
                    //Do online patch of LoLData from current version onwards
                    if (LolDataVersion != LatestLolDataVersion)
                    {
                        LogTextBox("Updating from " + LolDataVersion + " -> " + LatestLolDataVersion);
                        UpdateFrom(LolDataVersion, PackageManifest);
                        WriteLatestVersion(LatestLolDataVersion);
                    }
                    LogTextBox("Patching League of Legends.exe...");
                    //Everytime we update download all .exe and dll files
                    GetAllExe(PackageManifest);
                }
                LogTextBox("Done!");
                //*/
                #endregion lol_game_client

                

                FinishPatching();
            });

            bgThead.Start();
        }

        private string GetLolRootPath() 
        {
            var possiblePaths = new List<Tuple<string, string>>  
            {
                new Tuple<string, string>(@"HKEY_LOCAL_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\RIOT GAMES", "Path"),
                new Tuple<string, string>(@"HKEY_LOCAL_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\Wow6432Node\RIOT GAMES", "Path"),
                new Tuple<string, string>(@"HKEY_LOCAL_USER\Software\RIOT GAMES", "Path"),
                new Tuple<string, string>(@"HKEY_LOCAL_USER\Software\Wow6432Node\Riot Games", "Path"),
                new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Riot Games\League Of Legends", "Path"),
                new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games", "Path"),
                new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games\League Of Legends", "Path"),
                // Yes, a f*ckin whitespace after "Riot Games"..
                new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games \League Of Legends", "Path"),
            };
            foreach (var tuple in possiblePaths) 
            {
                var path = tuple.Item1;
                var valueName = tuple.Item2;
                try 
                {
                    var value = Microsoft.Win32.Registry.GetValue(path, valueName, string.Empty);
                    if (value != null && value.ToString() != string.Empty) 
                    {
                        return value.ToString();
                    }
                } catch { }
            }
            

            return string.Empty;
        }

        private void update(object sender, EventArgs e)
        {
            UpdateData legendaryupdatedata = new UpdateData();
            WebClient client = new WebClient();
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Temp")))
            {
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Temp"));
            }
            var downloadLink = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/downloadLink");
            var filename = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/filename");
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            string DownloadLocation = "https://github.com/eddy5641/LegendaryClient/releases/download/" + downloadLink;
            LogTextBox("Retreving Update Data from: " + DownloadLocation);
            client.DownloadFileAsync(new Uri(DownloadLocation), Path.Combine(Client.ExecutingDirectory, "Temp", "LegendaryClientUpdateFile.zip"));
            //client.DownloadFileAsync(new Uri(DownloadLocation), Path.Combine("temp", "1.0.1.2.zip"));
            //client.DownloadFileAsync(new Uri(DownloadLocation), filename);
        }

        private void updatebetausers(object sender, EventArgs e)
        {
            UpdateData legendaryupdatedata = new UpdateData();
            WebClient client = new WebClient();
            var Startup = Directory.GetParent(Client.ExecutingDirectory);
            if (!Directory.Exists(Path.Combine(Startup.ToString(), "Temp")))
            {
                Directory.CreateDirectory(Path.Combine(Startup.ToString(), "Temp"));
            }
            var downloadLink = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/downloadLink");
            var filename = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/filename");
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            string DownloadLocation = "https://github.com/eddy5641/LegendaryClient/releases/download/" + downloadLink;
            LogTextBox("Retreving Update Data from: " + DownloadLocation);
            client.DownloadFileAsync(new Uri(DownloadLocation), Path.Combine("Temp", "LegendaryClientBetaTesterUpdateFile.zip"));
            //client.DownloadFileAsync(new Uri(DownloadLocation), Path.Combine("temp", "1.0.1.2.zip"));
            //client.DownloadFileAsync(new Uri(DownloadLocation), filename);
        }

        public void Text_Changed(object sender, RoutedEventArgs e)
        {
            MessageOverlay overlay = new MessageOverlay();
                        //overlay.AcceptButton.Click += update;
            if (overlay.MessageTextBox.Text == "NO")
            {
                overlay.AcceptButton.Click -= update;
                overlay.Visibility = Visibility.Hidden;
            }
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                CurrentProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
                CurrentProgressLabel.Content = "Now downloading LegendaryClient";
            }));
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                CurrentProgressLabel.Content = "Download Completed";
                LogTextBox("Finished Download");
                LogTextBox("Starting Patcher. Please Wait");

                var location = Directory.GetParent(Client.ExecutingDirectory);
                var p = new System.Diagnostics.Process();
                p.StartInfo.WorkingDirectory = location.ToString();
                //p.StartInfo.FileName = Path.Combine(GameDirectory, "League of Legends.exe");
                p.StartInfo.FileName = "Patcher.exe";
                p.Start();
                
                Application.Current.Shutdown();
                System.Windows.Forms.Application.Exit();
                System.Environment.Exit(0);
            }));
            
        }

        void client_DownloadDDragon(object sender, AsyncCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                CurrentProgressLabel.Content = "Download Completed";
                LogTextBox("Finished Download");
                CurrentProgressBar.Value = 0;
            }));

        }


        private void FinishPatching()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TotalProgressLabel.Content = "100%";
                TotalProgessBar.Value = 100;
                SkipPatchButton.Content = "Play";
                CurrentProgressLabel.Content = "Finished Patching";
                CurrentStatusLabel.Content = "Ready To Play";
                SkipPatchButton.IsEnabled = true;
            }));
            
            LogTextBox("LegendaryClient Has Finished Patching");
            Client.Log("LegendaryClient Has Finished Patching");
        }

        private void LogTextBox(string s)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                PatchTextBox.Text += "[" + DateTime.Now.ToShortTimeString() + "] " + s + Environment.NewLine;
            }));
            Client.Log(s);
        }

        private void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

            foreach (var directory in Directory.GetDirectories(sourceDir))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }

        private string GetMd5()
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            FileInfo fi = null;
            FileStream stream = null;

            fi = new FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            stream = File.Open(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, FileMode.Open, FileAccess.Read);

            md5.ComputeHash(stream);

            stream.Close();

            string rtrn = "";
            for (int i = 0; i < md5.Hash.Length; i++)
            {
                rtrn += (md5.Hash[i].ToString("x2"));
            }
            return rtrn.ToUpper();
        }

        private void DeleteDirectoryRecursive(string path) 
        {
            foreach (var directory in Directory.GetDirectories(path)) {
                DeleteDirectoryRecursive(directory);
            }

            try {
                Directory.Delete(path, true);
            } catch (IOException) {
                Directory.Delete(path, true);
            } catch (UnauthorizedAccessException) {
                Directory.Delete(path, true);
            }
        }
        public void WriteLatestVersion(string fileDirectory)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            string dDirectory = fileDirectory;
            DirectoryInfo dInfo = new DirectoryInfo(dDirectory);
            DirectoryInfo[] subdirs = null;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch { return; }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
            {
                latestVersion = info.Name;
            }
            var VersionLOL = File.Create(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));
            VersionLOL.Write(encoding.GetBytes(latestVersion), 0, encoding.GetBytes(latestVersion).Length);
            VersionLOL.Close();
            LolDataVersion = latestVersion;
        }

        public void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }

        public void uncompressFile(string inFile, string outFile)
        {
            try 
            {
                int data = 0;
                int stopByte = -1;
                System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
                ZInputStream inZStream = new ZInputStream(System.IO.File.Open(inFile, System.IO.FileMode.Open, System.IO.FileAccess.Read));
                while (stopByte != (data = inZStream.Read()))
                {
                    byte _dataByte = (byte)data;
                    outFileStream.WriteByte(_dataByte);
                }
    
                inZStream.Close();
                outFileStream.Close(); 
            }
            catch 
            {
                Client.Log("Unable to find a file to uncompress");
            }
            
        }

        private void GetAllExe(string PackageManifest)
        {
            string[] FileMetaData = PackageManifest.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Skip(1).ToArray();
            foreach (string s in FileMetaData)
            {
                if (String.IsNullOrEmpty(s))
                {
                    continue;
                }
                //Remove size and type metadata
                string Location = s.Split(',')[0];
                //Get save position
                string SavePlace = Location.Split(new string[] { "/files/" }, StringSplitOptions.None)[1];
                if (SavePlace.EndsWith(".exe.compressed") || SavePlace.EndsWith(".dll.compressed"))
                {
                    LogTextBox("Downloading " + SavePlace);
                    using (WebClient newClient = new WebClient())
                    {
                        newClient.DownloadFile("http://l3cdn.riotgames.com/releases/live" + Location, Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", SavePlace));
                    }
                    uncompressFile(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", SavePlace), Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", SavePlace).Replace(".compressed", ""));
                    File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", SavePlace));
                }
            }
        }

        private void UpdateFrom(string version, string PackageManifest)
        {
            int CurrentVersionNumber = Convert.ToInt32(version.Split('.')[3]);
            string[] FileMetaData = PackageManifest.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Skip(1).ToArray();
            foreach (string s in FileMetaData)
            {
                if (String.IsNullOrEmpty(s))
                {
                    continue;
                }
                //Remove size and type metadata
                string Location = s.Split(',')[0];
                //Get save position
                string SavePlace = Location.Split(new string[] { "/files/" }, StringSplitOptions.None)[1];
                string[] VersionArray = Location.Split(new string[] { "/files/" }, StringSplitOptions.None)[0].Split('/');
                string Version = VersionArray[VersionArray.Length - 1];
                int VersionNumber = Convert.ToInt32(Version.Split('.')[3]);
                if (VersionNumber > CurrentVersionNumber) //Update if later than current version
                {
                    LogTextBox("Downloading " + SavePlace);
                    using (WebClient newClient = new WebClient())
                    {
                        try
                        {
                            newClient.DownloadFile("http://l3cdn.riotgames.com/releases/live" + Location, Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", SavePlace));
                        }
                        catch { }
                    }
                    uncompressFile(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", SavePlace), Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", SavePlace).Replace(".compressed", ""));
                    try
                    {
                        File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", SavePlace));
                    }
                    catch { }
                }
            }
        }

        private void CheckIfPatched()
        {
            string LolVersion = new WebClient().DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_NA");
            string CurrentLolVersion = System.IO.File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));
            LogTextBox("Latest version of League of Legends: " + LolVersion.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0]);
            LogTextBox("Your version of League of Legends: " + CurrentLolVersion.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0]);
            LoLDataIsUpToDate = LolVersion.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0] == CurrentLolVersion.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
            LolDataVersion = CurrentLolVersion.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
            LatestLolDataVersion = LolVersion.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
        }

        private bool ExpandRAF(string fileDirectory)
        {
            LogTextBox("Loading RAF Packages in " + fileDirectory);
            RAFMasterFileList list = new RAFMasterFileList(fileDirectory);
            LogTextBox("Expanding RAF packages. This will take a while (~20-30 minutes)...");
            LogTextBox("During this time computer performance may be affected. While patching, running applications should be closed or not in-use");
            int i = 0;
            foreach (var x in list.FileDictFull)
            {   
                string FileLastWritten = "";
                var RAFFile = x.Value[0];
                string n = Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client");
                foreach (string Directories in RAFFile.FileName.Split('/'))
                {
                    if (!Directories.Contains('.'))
                    {
                        if (!Directory.Exists(Path.Combine(n, Directories)))
                        {
                            Directory.CreateDirectory(Path.Combine(n, Directories));
                        }
                        n = Path.Combine(n, Directories);
                    }
                    else
                    {
                        BinaryWriter Writer = null;
                        try
                        {
                            Writer = new BinaryWriter(File.OpenWrite(Path.Combine(n, Directories)));

                            // Writer raw data                
                            Writer.Write(RAFFile.GetContent());
                            Writer.Flush();
                            Writer.Close();
                            FileLastWritten = Path.Combine(n, Directories);
                        }
                        catch
                        {
                            LogTextBox("Unable to write " + Path.Combine(n, Directories));
                        }
                    }
                }
                LogTextBox("(" + i + "/" + list.FileDictFull.Count + ") " + (((decimal)i / (decimal)list.FileDictFull.Count) * 100).ToString("N2") + "%");
                i += 1;
            }
            return true;
        }
    }
}