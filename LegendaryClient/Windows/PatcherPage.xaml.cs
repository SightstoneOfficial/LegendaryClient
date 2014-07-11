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

namespace LegendaryClient.Windows
{
    /// <summary>
    /// Interaction logic for PatcherPage.xaml
    /// </summary>
    public partial class PatcherPage : Page
    {
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
            Client.Log("[Debug]: Swiched to LoginPage");
        }

        private void StartPatcher()
        {
            Thread bgThead = new Thread(() =>
            {
                LogTextBox("Starting Patcher");
                Client.Log("Patcher Starting");

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
                Client.Log("[DEBUG]: MD5:" + CurrentMD5);
                string VersionString = "";
                try
                {
                    VersionString = client.DownloadString(new Uri("http://legendaryclient.com/update.html"));
                }
                catch
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Could not retrieve update files!";
                        Client.Log("[Warn]: Failed to retrieve update files");
                    }));

                    //return;
                }

                string[] VersionSplit = VersionString.Split('|');

                LogTextBox("Update data: " + VersionSplit[0] + "|" + VersionSplit[1]);
                Client.updateData = LegendaryUpdate.PopulateItems();

                #if !DEBUG //Dont patch client while in DEBUG
                UpdateData legendaryupdatedata = new UpdateData();
                var version = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/Version");
                LogTextBox("Most Up to date LegendaryClient Version: " + version);
                string versionAsString = version;
                var versiontoint = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/VersionAsInt");
                int VersionAsInt = Convert.ToInt32(versiontoint);

                if (VersionAsInt != Client.LegendaryClientReleaseNumber)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        MessageOverlay overlay = new MessageOverlay();
                        overlay.MessageTextBox.Text = "An update is available LegendaryClient";
                        overlay.MessageTitle.Content = "Update Notification";
                        overlay.AcceptButton.Content = "Update LegendaryClient";
                        overlay.MessageTextBox.TextChanged += Text_Changed;
                        Client.OverlayContainer.Content = overlay.Content;
                        Client.OverlayContainer.Visibility = Visibility.Visible;

                        CurrentProgressLabel.Content = "LegendaryClient Is Out of Date!";
                        
                    }));
                    LogTextBox("LegendaryClient Is Out of Date!");

                    return;
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
                //LogTextBox("LC Update Json Data: " + json);
                #endif

                //LogTextBox("LegendaryClient is up to date");
                //LogTextBox("LegendaryClient does not have a patcher downloader. Do not be worried by this.");
                
                //Client.Log("[Debug]: LegendaryClient Is Up To Date");

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
                Client.Log("[Debug]: DDragon Version (LOL Version) = " + DDragonVersion);

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
                    Directory.Delete(Path.Combine("Assets", "temp"), true);

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
                    if (Directory.Exists(AirLocation))
                    {
                        RetrieveCurrentInstallation = true;
                    }
                    else if (Directory.Exists(Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", AirLocation)))
                    {
                        RetrieveCurrentInstallation = true;
                        AirLocation = Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", AirLocation);
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

                #region lol_game_client

                LogTextBox("Searching For Lol Install");

                string Rad_Path;

                string LolPath1 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_USER\Software\RIOT GAMES\RADS", "Path", "").ToString();
                if (LolPath1 != "" || LolPath1 != null)
                {
                    //root.GetDirectories("*.*", System.IO.SearchOption.AllDirectories);
                    Rad_Path = Path.Combine(LolPath1, "RADS");

                }
                string LolPath2 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\Wow6432Node\RIOT GAMES\RADS", "Path", "").ToString();
                string LolPath3 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\RIOT GAMES\RADS", "Path", "").ToString();
                string LolPath4 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_USER\Software\Wow6432Node\Riot Games\RADS", "Path", "").ToString();
                string LolPath5 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\RIOT GAMES\RADS", "Path", "").ToString();
                string LolPath6 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games\RADS", "Path", "").ToString();
                string LolPath7 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Riot Games\League Of Legends", "Path", "").ToString();
                string LolPath8 = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games\RADS", "Path", "").ToString();

                
                if (!Directory.Exists("RADS"))
                {
                    Directory.CreateDirectory("RADS");
                }

                if (!File.Exists(Path.Combine("RADS", "VERSION_LOL")))
                {
                    var VersionGAME = File.Create(Path.Combine("RADS", "VERSION_LOL"));
                    VersionGAME.Write(encoding.GetBytes("0.0.0.0"), 0, encoding.GetBytes("0.0.0.0").Length);
                    VersionGAME.Close();
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        TotalProgressLabel.Content = "70%";
                        TotalProgessBar.Value = 70;
                    }));
                    
                }

                string LatestGame = patcher.GetLatestGame();
                LogTextBox("League Of Legends Version: " + LatestGame);
                string GameVersion = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));
                LogTextBox("Current League of Legends Version: " + GameVersion);
                RetrieveCurrentInstallation = false;
                string GameLocation = "";

                if (GameVersion == "0.0.0.0")
                {
                    LogTextBox("Checking for existing League of Legends Installation");
                    GameLocation = Path.Combine("League of Legends", "RADS");
                    string rootPath = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Riot Games\League Of Legends", "Path", "").ToString();
                    if (Directory.Exists(GameLocation))
                    {
                        RetrieveCurrentInstallation = true;
                    }
                    else if (Directory.Exists(Path.Combine(rootPath, "RADS")))
                    {
                        RetrieveCurrentInstallation = true;
                        GameLocation = Path.Combine(rootPath, "RADS");
                    }
                    else
                    {
                        LogTextBox("Unable to find existing League of Legends. Copy your League of Legends folder into + "
                            + Client.ExecutingDirectory
                            + " to make the patching process quicker");
                    }

                    if (RetrieveCurrentInstallation)
                    {
                        LogTextBox("Getting League Of Legends from " + GameLocation);
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            CurrentProgressLabel.Content = "Copying League of Legends";
                        }));
                        GameVersion = patcher.GetCurrentGameInstall(GameLocation);
                        LogTextBox("Retrieved currently installed League of Legends");
                        LogTextBox("Current League of Legends Version: " + GameLocation);
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            TotalProgressLabel.Content = "80%";
                            TotalProgessBar.Value = 80;
                        }));
                        
                    }
                    
                    
                }

                if (GameVersion != LatestGame)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Retrieving League of Legends";
                    }));
                }
                //*/
                #endregion lol_game_client

                

                FinishPatching();
            });

            bgThead.Start();
        }

        private void update(object sender, EventArgs e)
        {
            UpdateData legendaryupdatedata = new UpdateData();
            WebClient client = new WebClient();
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "temp")))
            {
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "temp"));
            }
            var downloadLink = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/downloadLink");
            var filename = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/filename");
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            string DownloadLocation = "https://github.com/eddy5641/LegendaryClient/releases/download/" + downloadLink;
            LogTextBox("Retreving Update Data from: " + DownloadLocation);
            client.DownloadFileAsync(new Uri(DownloadLocation), Path.Combine("temp", "1.0.1.2.zip"));
            //client.DownloadFileAsync(new Uri(DownloadLocation), Path.Combine("temp", "1.0.1.2.zip"));
            //client.DownloadFileAsync(new Uri(DownloadLocation), filename);
        }

        private void updatebetausers(object sender, EventArgs e)
        {
            UpdateData legendaryupdatedata = new UpdateData();
            WebClient client = new WebClient();
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "temp")))
            {
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "temp"));
            }
            var downloadLink = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/downloadLink");
            var filename = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/filename");
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
            string DownloadLocation = "https://github.com/eddy5641/LegendaryClient/releases/download/" + downloadLink;
            LogTextBox("Retreving Update Data from: " + DownloadLocation);
            client.DownloadFileAsync(new Uri(DownloadLocation), Path.Combine("temp", "1.0.1.2.zip"));
            //client.DownloadFileAsync(new Uri(DownloadLocation), Path.Combine("temp", "1.0.1.2.zip"));
            //client.DownloadFileAsync(new Uri(DownloadLocation), filename);
        }

        public void Text_Changed(object sender, RoutedEventArgs e)
        {
            MessageOverlay overlay = new MessageOverlay();
                        //overlay.AcceptButton.Click += update;
            if (overlay.MessageTextBox.Text != "NO")
            {
                overlay.AcceptButton.Click += update;
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
            }));
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                CurrentProgressLabel.Content = "Download Completed";
                LogTextBox("Finished Download");
                LogTextBox("Starting Patcher. Please Wait");
                System.Diagnostics.Process.Start("Patcher.exe");
                Environment.Exit(0);
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
            Client.Log("[Debug]: LegendaryClient Has Finished Patching");

            
            /*
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Client.SwitchPage(new LoginPage());
            }));*/
        }

        private void LogTextBox(string s)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                PatchTextBox.Text += "[" + DateTime.Now.ToShortTimeString() + "] " + s + Environment.NewLine;
            }));
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
        //internal static string DDragonVersion = DDragonVersion;
        
    }
}