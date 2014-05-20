using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Patcher;
using System;
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
                LogTextBox("LegendaryClient tested for League of Legends V 4.5.13");
                Client.Log("Patcher Starting");

                WebClient client = new WebClient();
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
                    return;
                }

                string[] VersionSplit = VersionString.Split('|');

                LogTextBox("Update data: " + VersionSplit[0] + "|" + VersionSplit[1]);

#if !DEBUG //Dont patch client while in DEBUG
                if (VersionSplit.Length == 3)
                {
                    string[] versionArray = VersionString.Split('|');
                    if (VersionSplit[0] != CurrentMD5)
                    {
                        LogTextBox("LegendaryClient needs to be updated");
                        /*Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            CurrentStatusLabel.Content = "Downloading latest LegendaryClient...";
                        }));

                        client.DownloadFile(versionArray[2], "COL.ZIP");
                        Directory.CreateDirectory("Patch");
                        System.IO.Compression.ZipFile.ExtractToDirectory("COL.ZIP", "Patch");
                        File.Delete("COL.ZIP");
                        System.Diagnostics.Process.Start("Patcher.exe");
                        Environment.Exit(0);*/
                    }
                }
#endif
                //LogTextBox("LegendaryClient is up to date");
                //LogTextBox("LegendaryClient does not have a patcher downloader. Do not be worried by this.");
                LogTextBox("LegendaryClient is up to date");
                Client.Log("[Debug]: LegendaryClient Is Up To Date");

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
                    if (Directory.Exists(GameLocation))
                    {
                        RetrieveCurrentInstallation = true;
                    }
                    else if (Directory.Exists(Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", GameLocation)))
                    {
                        RetrieveCurrentInstallation = true;
                        GameLocation = Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", GameLocation);
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

                #endregion lol_game_client

                

                FinishPatching();
            });

            bgThead.Start();
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