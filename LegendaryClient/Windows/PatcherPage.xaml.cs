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
using Microsoft.Win32;

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
            DevKey.TextChanged += DevKey_TextChanged;
            StartPatcher();
            Client.Log("LegendaryClient Started Up Successfully");
        }

        void DevKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DevKey.Text.StartsWith("!~betakey-"))
                if (DevKey.Text.EndsWith("~!"))
                    GetDev(DevKey.Text.Replace("!~betakey-", "").Replace("~!", ""));
        }

        private void GetDev(string DevKey)
        {
            if (DevKey.Length != 20)
                return;
            bool Auth = false;
            var client = new WebClient();
            string KeyPlayer = client.DownloadString("http://eddy5641.github.io/LegendaryClient/BetaUsers");
            string[] Players = KeyPlayer.Split(new string[] { Environment.NewLine }, 0, StringSplitOptions.RemoveEmptyEntries);
            foreach (string Beta in Players)
            {
                string[] BetaKey = Beta.Split(',');
                if (DevKey == BetaKey[0])
                {
                    Auth = true;
                    Welcome.Text = "Welcome " + BetaKey[1];
                    Welcome.Visibility = Visibility.Visible;
                }
            }
            if (Auth == false)
            {
                this.DevKey.Text = "";
            }
            
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

                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    TotalProgressLabel.Content = "20%";
                    TotalProgessBar.Value = 20;
                }));
                #region idk

                client = new WebClient();
                client.DownloadFile(new Uri("http://eddy5641.github.io/LegendaryClient/Login/Login.mp3"), Path.Combine(Client.ExecutingDirectory, "Login.mp3"));
                client.DownloadFile(new Uri("http://eddy5641.github.io/LegendaryClient/Login/Login.mp4"), Path.Combine(Client.ExecutingDirectory, "Login.mp4"));
            
                #endregion idk

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
                WebClient UpdateClient = new WebClient();
                string Release = UpdateClient.DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting");
                string LatestVersion = Release.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
                
                if (AirVersion != LatestVersion)
                {
                    //Download Air Assists from riot
                    string Package = UpdateClient.DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/" + LatestVersion + "/packages/files/packagemanifest");
                    GetAllPngs(Package);
                    if (File.Exists(Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite")))
                        File.Delete(Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite"));
                    UpdateClient.DownloadFile(new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/" + LatestVersion + "/files/assets/data/gameStats/gameStats_en_US.sqlite"), Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite"));

                    if (File.Exists(System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR")))
                        File.Delete(System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                    var file = File.Create(System.IO.Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                    file.Write(encoding.GetBytes(LatestVersion), 0, encoding.GetBytes(LatestVersion).Length);
                    file.Close();
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
                    Client.LOLCLIENTVERSION = LolVersion2;
                    Client.Location = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln", "releases", GameClientSln, "deploy");
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
                #endregion lol_game_client

                

                FinishPatching(LatestVersion);
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
            
                OpenFileDialog FindLeagueDialog = new OpenFileDialog();

                if (!Directory.Exists(Path.Combine("C:\\", "Riot Games", "League of Legends")))
                {
                    FindLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Program Files (x86)", "GarenaLoL", "GameData", "Apps", "LoL");
                }
                else
                {
                FindLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Riot Games", "League of Legends");
                }
                FindLeagueDialog.DefaultExt = ".exe";
                FindLeagueDialog.Filter = "League of Legends Launcher|lol.launcher*.exe|Garena Launcher|lol.exe";

                Nullable<bool> result = FindLeagueDialog.ShowDialog();
            if (result == true)
            {
                return FindLeagueDialog.FileName.Replace("lol.launcher.exe", "").Replace("lol.launcher.admin.exe", "");
            }
            else
                return string.Empty;
        }

        [Obsolete]
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

        [Obsolete]
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
        [Obsolete]
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


        private void FinishPatching(string LatestVersion)
        {
            WebClient UpdateClient = new WebClient();
            UpdateClient.DownloadFile("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/" + LatestVersion + "/files/assets/data/gameStats/gameStats_en_US.sqlite", Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite"));
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
                PatchTextBox.ScrollToEnd();
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
        [Obsolete]
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
        private void GetAllPngs(string PackageManifest)
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
                if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "champions")))
                    Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "champions"));
                if (SavePlace.EndsWith(".jpg") || SavePlace.EndsWith(".png"))
                {
                    if (SavePlace.Contains("assets/images/champions/"))
                    {                        
                        using (WebClient newClient = new WebClient())
                        {
                            string SaveName = Location.Split(new string[] { "/champions/" }, StringSplitOptions.None)[1];
                            LogTextBox("Downloading " + SaveName + " from http://l3cdn.riotgames.com");
                            newClient.DownloadFile("http://l3cdn.riotgames.com/releases/live" + Location, Path.Combine(Client.ExecutingDirectory, "Assets", "champions", SaveName));
                        }
                    }                    
                }
            }
        }
        [Obsolete]
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
        [Obsolete]
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
        [Obsolete]
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