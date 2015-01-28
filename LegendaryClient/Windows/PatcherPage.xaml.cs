#region

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ComponentAce.Compression.Libs.zlib;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Patcher;
using LegendaryClient.Logic.UpdateRegion;
using LegendaryClient.Properties;
using Microsoft.Win32;
using RAFlibPlus;
using System.Xml;

#endregion

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for PatcherPage.xaml
    /// </summary>
    public partial class PatcherPage
    {
        //#FF2E2E2E
        internal static bool LoLDataIsUpToDate;
        internal static string LatestLolDataVersion = string.Empty;
        internal static string LolDataVersion = string.Empty;

        public PatcherPage()
        {
            InitializeComponent();
            Change();

            UpdateRegionComboBox.SelectedValue = Settings.Default.updateRegion != string.Empty ? Settings.Default.updateRegion : "Live";
            Client.UpdateRegion = (string)UpdateRegionComboBox.SelectedValue;

            bool x = Settings.Default.DarkTheme;
            if (!x)
            {
                var bc = new BrushConverter();
                PatchTextBox.Background = (Brush) bc.ConvertFrom("#FFECECEC");
                DevKey.Background = (Brush) bc.ConvertFrom("#FFECECEC");
                PatchTextBox.Foreground = (Brush) bc.ConvertFrom("#FF1B1919");
            }
            //DevKey.TextChanged += DevKey_TextChanged;
#if !DEBUG
            UpdateSplash();
#endif
            StartPatcher();
            Client.Log("LegendaryClient Started Up Successfully");
        }

        public void Change()
        {
            var themeAccent = new ResourceDictionary
            {
                Source = new Uri(Settings.Default.Theme)
            };
            Resources.MergedDictionaries.Add(themeAccent);
        }

        private void DevSkip_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new LoginPage());
            Client.Log("Swiched to LoginPage with DevSkip");
        }

        private void SkipPatchButton_Click(object sender, RoutedEventArgs e)
        {
            //var updateClient = new WebClient();
            Client.SwitchPage(new LoginPage());
        }

#if !DEBUG
        private void UpdateSplash()
        {
            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(Path.Combine(Client.ExecutingDirectory, "LCStartUpSplash.exe"));
            string latestSplash = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/LatestSplash.Version").Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
            if (versionInfo.FileVersion != latestSplash)
            {

            }
        }
#endif
        private void StartPatcher()
        {
            try
            {
                var bgThead = new Thread(() =>
                {
                    LogTextBox("Starting Patcher");

                    var client = new WebClient();
                    client.DownloadProgressChanged += client_DownloadProgressChanged;
                    client.DownloadFileCompleted += client_DownloadDDragon;
                    client.DownloadProgressChanged +=
                        (o, e) => Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            double bytesIn = double.Parse(e.BytesReceived.ToString(CultureInfo.InvariantCulture));
                            double totalBytes =
                                double.Parse(e.TotalBytesToReceive.ToString(CultureInfo.InvariantCulture));
                            double percentage = bytesIn/totalBytes*100;
                            CurrentProgressLabel.Content = "Downloaded " + e.BytesReceived + " of " +
                                                           e.TotalBytesToReceive;
                            CurrentProgressBar.Value =
                                int.Parse(Math.Truncate(percentage).ToString(CultureInfo.InvariantCulture));
                        }));

                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        TotalProgressLabel.Content = "20%";
                        TotalProgessBar.Value = 20;
                    }));

                    #region idk

                    client = new WebClient();
                    if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp3")))
                        client.DownloadFile(
                            new Uri(
                                "https://s12.solidfilesusercontent.com/MDE1MWYxZGJmYWFhNzJmNGQ2N2ZhOWE0NzU4Yjk2ZDYwZjY3MGU2OToxWHp3OTk6dUllemo3WDM0RnlScUgxZk1YWXpKYmN0RXBn/7a0671ed14/Login.mp3"),
                            Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp3"));

                    if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp4")))
                        client.DownloadFile(
                            new Uri(
                                "https://s8.solidfilesusercontent.com/MzkxMTBjOTllZDczMTBjZDUwNzgwOTc1NTYwZmY1Nzg2YThkZDI5MzoxWHp2eE86alBDQXBkU1FuNmt6R3dsTzcycEtoOXpGdVZr/a38bbf759c/Login.mp4"),
                            Path.Combine(Client.ExecutingDirectory, "Client", "Login.mp4"));

                    #endregion idk

                    #region DDragon

                    var encoding = new ASCIIEncoding();
                    if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets")))
                        Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets"));

                    if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_DDRagon")))
                    {
                        FileStream versionLol =
                            File.Create(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_DDRagon"));
                        versionLol.Write(encoding.GetBytes("0.0.0"), 0, encoding.GetBytes("0.0.0").Length);

                        versionLol.Close();
                    }


                    var patcher = new RiotPatcher();
                    string dDragonDownloadUrl = patcher.GetDragon();
                    if (!String.IsNullOrEmpty(dDragonDownloadUrl))
                    {
                        LogTextBox("DataDragon Version: " + patcher.DDragonVersion);
                        string dDragonVersion =
                            File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_DDragon"));
                        LogTextBox("Current DataDragon Version: " + dDragonVersion);

                        Client.Version = dDragonVersion;
                        Client.Log("DDragon Version (LOL Version) = " + dDragonVersion);

                        LogTextBox("Client Version: " + Client.Version);

                        if (patcher.DDragonVersion != dDragonVersion)
                        {
                            try
                            {
                                if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "temp")))
                                    Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "temp"));

                                Dispatcher.BeginInvoke(DispatcherPriority.Input,
                                    new ThreadStart(() => { CurrentProgressLabel.Content = "Downloading DataDragon"; }));
                                client.DownloadFile(dDragonDownloadUrl,
                                    Path.Combine(Client.ExecutingDirectory, "Assets",
                                        "dragontail-" + patcher.DDragonVersion + ".tgz"));


                                Dispatcher.BeginInvoke(DispatcherPriority.Input,
                                    new ThreadStart(() => { CurrentProgressLabel.Content = "Extracting DataDragon"; }));

                                Stream inStream =
                                    File.OpenRead(Path.Combine(Client.ExecutingDirectory, "Assets",
                                        "dragontail-" + patcher.DDragonVersion + ".tgz"));

                                using (var gzipStream = new GZipInputStream(inStream))
                                {
                                    TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                                    tarArchive.ExtractContents(Path.Combine(Client.ExecutingDirectory, "Assets", "temp"));
                                    //tarArchive.Close();
                                }
                                inStream.Close();

                                Copy(
                                    Path.Combine(Client.ExecutingDirectory, "Assets", "temp", patcher.DDragonVersion,
                                        "data"), Path.Combine(Client.ExecutingDirectory, "Assets", "data"));
                                Copy(
                                    Path.Combine(Client.ExecutingDirectory, "Assets", "temp", patcher.DDragonVersion,
                                        "img"), Path.Combine(Client.ExecutingDirectory, "Assets"));
                                DeleteDirectoryRecursive(Path.Combine(Client.ExecutingDirectory, "Assets", "temp"));

                                FileStream versionDDragon =
                                    File.Create(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_DDRagon"));
                                versionDDragon.Write(encoding.GetBytes(patcher.DDragonVersion), 0,
                                    encoding.GetBytes(patcher.DDragonVersion).Length);

                                Client.Version = dDragonVersion;
                                versionDDragon.Close();
                            }
                            catch
                            {
                                Client.Log(
                                    "Probably updated version number without actually uploading the files.");
                            }
                        }
                    }
                    else
                    {
                        LogTextBox(
                            "Failed to get DDragon version. Either not able to be found or unknown error (most likely the website is in maitenance, please try again in an hour or so)");
                        LogTextBox(
                            "Continuing could cause errors. Report this as an issue if it occurs again in a few hours.");
                    }

                    #endregion DDragon

                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        TotalProgressLabel.Content = "40%";
                        TotalProgessBar.Value = 40;
                    }));

                    // Try get LoL path from registry

                    //A string that looks like C:\Riot Games\League of Legends\
                    string lolRootPath = GetLolRootPath(false);

                    #region lol_air_client

                    if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR")))
                    {
                        FileStream versionAir =
                            File.Create(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                        versionAir.Write(encoding.GetBytes("0.0.0.0"), 0, encoding.GetBytes("0.0.0.0").Length);
                        versionAir.Close();
                    }

                    BaseUpdateRegion updateRegion = BaseUpdateRegion.GetUpdateRegion(Client.UpdateRegion);
                    string latestAir = patcher.GetListing(updateRegion.AirListing);
                    LogTextBox("Air Assets Version: " + latestAir);
                    string airVersion =
                        File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                    LogTextBox("Current Air Assets Version: " + airVersion);
                    var updateClient = new WebClient();
                    if (airVersion != latestAir)
                    {
                        //Download Air Assists from riot
                        try
                        {
                            string airManifestLink = updateRegion.AirManifest + "releases/" + latestAir + "/packages/files/packagemanifest";
                            string[] allFiles = patcher.GetManifest(airManifestLink);
                            int i = 0;
                            while (!allFiles[i].Contains("gameStats_en_US.sqlite"))
                            {
                                i++;
                            }

                            updateClient.DownloadFile(new Uri(updateRegion.BaseLink + allFiles[i].Split(',')[0]), Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite"));

                            GetAllPngs(allFiles);
                            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR")))
                            {
                                File.Delete(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                            }

                            using (
                                FileStream file =
                                    File.Create(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR")))
                                file.Write(encoding.GetBytes(latestAir), 0,
                                    encoding.GetBytes(latestAir).Length);
                        }
                        catch (Exception e)
                        {
                            Client.Log(e.Message);
                        }
                    }

                    if (airVersion != latestAir)
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            SkipPatchButton.IsEnabled = true;
                            CurrentProgressLabel.Content = "Retrieving Air Assets";
                        }));

                    #endregion lol_air_client

                    //string GameVersion = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));

                    #region lol_game_client

                    LogTextBox("Trying to detect League of Legends GameClient");
                    LogTextBox("League of Legends is located at: " + lolRootPath);
                    //RADS\solutions\lol_game_client_sln\releases
                    string gameLocation = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln",
                        "releases");

                    string solutionListing = 
                        patcher.GetListing(
                            updateRegion.SolutionListing);

                    string solutionVersion = solutionListing.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
                    LogTextBox("Latest League of Legends GameClient: " + solutionVersion);
                    LogTextBox("Checking if League of Legends is Up-To-Date");

                    bool toExit = false;
                    if (Client.UpdateRegion == "Garena")
                    {
                        XmlReader reader = XmlReader.Create("http://updateres.garenanow.com/im/versions.xml");
                        string garenaVersion = "";
                        while (reader.Read())
                        {
                            if (reader.GetAttribute("name") == "lol")
                            {
                                garenaVersion = reader.GetAttribute("latest_version");
                                break;
                            }
                        }
                        try
                        {
                            if (garenaVersion == File.ReadAllText(Path.Combine(Path.GetDirectoryName(lolRootPath), "lol.version")))
                            {
                                LogTextBox("League of Legends is Up-To-Date");
                                Client.Location = Path.Combine(lolRootPath, "Game");
                                Client.RootLocation = lolRootPath;
                            }
                            else
                            {
                                LogTextBox("League of Legends is not Up-To-Date. Please Update League Of Legends");
                                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                                {
                                    SkipPatchButton.IsEnabled = true;
                                    FindClientButton.Visibility = Visibility.Visible;
                                }));
                                toExit = true;
                            }
                        }
                        catch
                        {
                            LogTextBox("Can't find League of Legends version file. Make sure you select correct update region.");
                            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                SkipPatchButton.IsEnabled = true;
                                FindClientButton.Visibility = Visibility.Visible;
                            }));
                            toExit = true;
                        }
                    }
                    else if (Directory.Exists(Path.Combine(gameLocation, solutionVersion)))
                    {
                        LogTextBox("League of Legends is Up-To-Date");
                        Client.Location = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln",
                            "releases", solutionVersion, "deploy");
                        Client.RootLocation = lolRootPath;
                    }
                    else
                    {
                        LogTextBox("League of Legends is not Up-To-Date. Please Update League Of Legends");
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            SkipPatchButton.IsEnabled = true;
                            FindClientButton.Visibility = Visibility.Visible;
                        }));
                        toExit = true;
                    }

                    #endregion lol_game_client

                    if (toExit)
                        return;

                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        TotalProgressLabel.Content = "100%";
                        TotalProgessBar.Value = 100;
                        SkipPatchButton.Content = "Play";
                        CurrentProgressLabel.Content = "Finished Patching";
                        CurrentStatusLabel.Content = "Ready To Play";
                        SkipPatchButton.IsEnabled = true;
                        SkipPatchButton_Click(null, null);
                    }));

                    LogTextBox("LegendaryClient Has Finished Patching");
                }) {IsBackground = true};

                bgThead.Start();
            }
            catch (Exception e)
            {
                Client.Log(e.Message + " - in PatcherPage updating progress.");
            }
        }

        private string GetLolRootPath(bool restart)
        {
            if (!restart)
            {
                switch(Client.UpdateRegion)
                {
                    case "PBE": if (Settings.Default.PBELocation != string.Empty)
                        return Settings.Default.PBELocation;
                        else break;
                    case "Live": if (Settings.Default.LiveLocation != string.Empty)
                            return Settings.Default.LiveLocation;
                        else break;
                    case "Korea": if (Settings.Default.KRLocation != string.Empty)
                            return Settings.Default.KRLocation;
                        else break;
                    case "Garena": if (Settings.Default.GarenaLocation != string.Empty)
                            return Settings.Default.GarenaLocation;
                        else break;
                    default :
                        break;
                }
                var possiblePaths = new List<Tuple<string, string>>
                {
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
                    string path = tuple.Item1;
                    string valueName = tuple.Item2;
                    try
                    {
                        var value = Registry.GetValue(path, valueName, string.Empty);
                        if (value != null && value.ToString() != string.Empty)
                        {
                            var regKey = Registry.CurrentUser.CreateSubKey("LegendaryClient");
                            if (regKey != null)
                            {
                                regKey.SetValue(
                                    value.ToString().Contains("lol.exe") ? "GarenaLocation" : "LoLLocation",
                                    value.ToString());
                                regKey.Close();
                            }
                            return value.ToString();
                        }
                    }
                    catch {
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

            bool? result = findLeagueDialog.ShowDialog();
            if (result != true)
                return string.Empty;

            RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\RIOT GAMES");
            if (key != null)
                key.SetValue("Path",
                    findLeagueDialog.FileName.Replace("lol.launcher.exe", string.Empty).Replace("lol.launcher.admin.exe", string.Empty));

            if (restart)
                LogTextBox("Saved value, please restart the client to login.");

            return findLeagueDialog.FileName.Replace("lol.launcher.exe", string.Empty).Replace("lol.launcher.admin.exe", string.Empty);
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn/totalBytes*100;

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                CurrentProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString(CultureInfo.InvariantCulture));
                CurrentProgressLabel.Content = "Now downloading LegendaryClient";
            }));
        }

        [Obsolete]
        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                CurrentProgressLabel.Content = "Download Completed";
                LogTextBox("Finished Download");
                LogTextBox("Starting Patcher. Please Wait");

                DirectoryInfo location = Directory.GetParent(Client.ExecutingDirectory);
                var p = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = location.ToString(),
                        FileName = "Patcher.exe"
                    }
                };
                //p.StartInfo.FileName = Path.Combine(GameDirectory, "League of Legends.exe");
                p.Start();

                Application.Current.Shutdown();
                Environment.Exit(0);
            }));
        }

        private void client_DownloadDDragon(object sender, AsyncCompletedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                CurrentProgressLabel.Content = "Download Completed";
                LogTextBox("Finished Download");
                CurrentProgressBar.Value = 0;
            }));
        }

        private void UpdateClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
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
                PatchTextBox.ScrollToEnd();
            }));
            Client.Log(s);
        }

        private void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (string file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

            foreach (string directory in Directory.GetDirectories(sourceDir))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }

        private string GetMd5()
        {
            var md5 = new MD5CryptoServiceProvider();
            FileInfo fi = null;

            fi = new FileInfo(Process.GetCurrentProcess().MainModule.FileName);
            FileStream stream = File.Open(Process.GetCurrentProcess().MainModule.FileName, FileMode.Open,
                FileAccess.Read);

            md5.ComputeHash(stream);

            stream.Close();

            string rtrn = md5.Hash.Aggregate(string.Empty, (current, t) => current + (t.ToString("x2")));
            return rtrn.ToUpper();
        }

        private void DeleteDirectoryRecursive(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
                DeleteDirectoryRecursive(directory);

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        public void WriteLatestVersion(string fileDirectory)
        {
            var encoding = new ASCIIEncoding();
            string dDirectory = fileDirectory;
            var dInfo = new DirectoryInfo(dDirectory);
            DirectoryInfo[] subdirs;
            try
            {
                subdirs = dInfo.GetDirectories();
            }
            catch
            {
                return;
            }
            string latestVersion = "0.0.1";
            foreach (DirectoryInfo info in subdirs)
                latestVersion = info.Name;

            FileStream versionLol = File.Create(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));
            versionLol.Write(encoding.GetBytes(latestVersion), 0, encoding.GetBytes(latestVersion).Length);
            versionLol.Close();
            LolDataVersion = latestVersion;
        }

        public void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
                output.Write(buffer, 0, len);

            output.Flush();
        }

        public void UncompressFile(string inFile, string outFile)
        {
            try
            {
                int data;
                const int stopByte = -1;
                var outFileStream = new FileStream(outFile, FileMode.Create);
                var inZStream = new ZInputStream(File.Open(inFile, FileMode.Open, FileAccess.Read));
                while (stopByte != (data = inZStream.Read()))
                {
                    var dataByte = (byte) data;
                    outFileStream.WriteByte(dataByte);
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
        private void GetAllExe(string packageManifest)
        {
            string[] FileMetaData =
                packageManifest.Split(new[] {Environment.NewLine}, StringSplitOptions.None).Skip(1).ToArray();
            foreach (string s in FileMetaData)
            {
                if (String.IsNullOrEmpty(s))
                    continue;

                //Remove size and type metadata
                string location = s.Split(',')[0];
                //Get save position
                string savePlace = location.Split(new[] {"/files/"}, StringSplitOptions.None)[1];
                if (!savePlace.EndsWith(".exe.compressed") && !savePlace.EndsWith(".dll.compressed"))
                    continue;

                LogTextBox("Downloading " + savePlace);
                using (var newClient = new WebClient())
                    newClient.DownloadFile("http://l3cdn.riotgames.com/releases/live" + location,
                        Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", savePlace));

                UncompressFile(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", savePlace),
                    Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", savePlace)
                        .Replace(".compressed", string.Empty));
                File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", savePlace));
            }
        }

        private void GetAllPngs(string[] packageManifest)
        {
            string[] fileMetaData =
                packageManifest.Skip(1).ToArray();
            var currentVersion =
                new Version(File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR")));

            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "champions")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "champions"));

            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds"));

            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "champions")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "champions"));

            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "ambient")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "ambient"));

            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon"));

            foreach (string s in fileMetaData)
            {
                if (String.IsNullOrEmpty(s))
                    continue;

                //Remove size and type metadata
                string location = s.Split(',')[0];
                //Get save position
                var version = new Version(location.Split(new[] {"/releases/", "/files/"}, StringSplitOptions.None)[1]);
                if (version <= currentVersion)
                    continue;
                BaseUpdateRegion updateRegion = BaseUpdateRegion.GetUpdateRegion(Client.UpdateRegion);

                string savePlace = location.Split(new[] {"/files/"}, StringSplitOptions.None)[1];
                if (!savePlace.EndsWith(".jpg") && !savePlace.EndsWith(".png") && !savePlace.EndsWith(".mp3"))
                    continue;

                if (savePlace.Contains("assets/images/champions/"))
                {
                    using (var newClient = new WebClient())
                    {
                        string saveName = location.Split(new[] {"/champions/"}, StringSplitOptions.None)[1];
                        LogTextBox("Downloading " + saveName + " from http://l3cdn.riotgames.com");
                        newClient.DownloadFile(updateRegion.BaseLink + location,
                            Path.Combine(Client.ExecutingDirectory, "Assets", "champions", saveName));
                    }
                }
                else if (savePlace.Contains("assets/images/abilities/"))
                {
                    using (var newClient = new WebClient())
                    {
                        string saveName = location.Split(new[] {"/abilities/"}, StringSplitOptions.None)[1];
                        LogTextBox("Downloading " + saveName + " from http://l3cdn.riotgames.com");
                        newClient.DownloadFile(updateRegion.BaseLink + location,
                            saveName.ToLower().Contains("passive")
                                ? Path.Combine(Client.ExecutingDirectory, "Assets", "passive", saveName)
                                : Path.Combine(Client.ExecutingDirectory, "Assets", "spell", saveName));
                    }
                }
                else if (savePlace.Contains("assets/storeImages/content/summoner_icon/"))
                {
                    using (var newClient = new WebClient())
                    {
                        string saveName = location.Split(new[] { "/summoner_icon/" }, StringSplitOptions.None)[1];
                        LogTextBox("Downloading " + saveName + " from http://l3cdn.riotgames.com");
                        newClient.DownloadFile(updateRegion.BaseLink + location,
                            saveName.ToLower().Contains("_")
                                ? Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", saveName.Split(new[] {"_"}, StringSplitOptions.None)[0] + ".png")
                                : Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", saveName.Replace("profileIcon", string.Empty)));
                    }
                } 
                else if (savePlace.Contains("assets/sounds/"))
                {
                    using (var newClient = new WebClient())
                    {
                        if (savePlace.Contains("en_US/champions/"))
                        {
                            string saveName = location.Split(new[] {"/champions/"}, StringSplitOptions.None)[1];
                            LogTextBox("Downloading " + saveName + " from http://l3cdn.riotgames.com");
                            newClient.DownloadFile(updateRegion.BaseLink + location,
                                Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "champions",
                                    saveName));
                        }
                        else if (savePlace.Contains("assets/sounds/ambient"))
                        {
                            string saveName = location.Split(new[] {"/ambient/"}, StringSplitOptions.None)[1];
                            LogTextBox("Downloading " + saveName + " from http://l3cdn.riotgames.com");
                            newClient.DownloadFile(updateRegion.BaseLink + location,
                                Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "ambient", saveName));
                        }
                        else if (savePlace.Contains("assets/sounds/matchmakingqueued.mp3"))
                        {
                            LogTextBox("Downloading matchmakingqueued.mp3 from http://l3cdn.riotgames.com");
                            newClient.DownloadFile(updateRegion.BaseLink + location,
                                Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "matchmakingqueued.mp3"));
                        }
                    }
                }
            }
        }

        [Obsolete]
        private void UpdateFrom(string version, string packageManifest)
        {
            int currentVersionNumber = Convert.ToInt32(version.Split('.')[3]);
            string[] fileMetaData =
                packageManifest.Split(new[] {Environment.NewLine}, StringSplitOptions.None).Skip(1).ToArray();
            foreach (string s in fileMetaData)
            {
                if (String.IsNullOrEmpty(s))
                    continue;

                //Remove size and type metadata
                string location = s.Split(',')[0];
                //Get save position
                string savePlace = location.Split(new[] {"/files/"}, StringSplitOptions.None)[1];
                string[] versionArray = location.Split(new[] {"/files/"}, StringSplitOptions.None)[0].Split('/');
                string Version = versionArray[versionArray.Length - 1];
                int versionNumber = Convert.ToInt32(Version.Split('.')[3]);
                if (versionNumber <= currentVersionNumber)
                    continue;

                LogTextBox("Downloading " + savePlace);
                using (var newClient = new WebClient())
                {
                    try
                    {
                        newClient.DownloadFile("http://l3cdn.riotgames.com/releases/live" + location,
                            Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", savePlace));
                    }
                    catch
                    {
                    }
                }
                UncompressFile(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", savePlace),
                    Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", savePlace)
                        .Replace(".compressed", string.Empty));
                try
                {
                    File.Delete(Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client", savePlace));
                }
                catch
                {
                }
            }
        }

        [Obsolete]
        private void CheckIfPatched()
        {
            string lolVersion =
                new WebClient().DownloadString(
                    "http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_NA");
            string currentLolVersion = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "RADS", "VERSION_LOL"));
            LogTextBox("Latest version of League of Legends: " +
                       lolVersion.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0]);
            LogTextBox("Your version of League of Legends: " +
                       currentLolVersion.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0]);
            LoLDataIsUpToDate = lolVersion.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0] ==
                                currentLolVersion.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
            LolDataVersion = currentLolVersion.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
            LatestLolDataVersion = lolVersion.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
        }

        [Obsolete]
        private bool ExpandRAF(string fileDirectory)
        {
            LogTextBox("Loading RAF Packages in " + fileDirectory);
            var list = new RAFMasterFileList(fileDirectory);
            LogTextBox("Expanding RAF packages. This will take a while (~20-30 minutes)...");
            LogTextBox(
                "During this time computer performance may be affected. While patching, running applications should be closed or not in-use");
            int i = 0;
            foreach (var x in list.FileDictFull)
            {
                string FileLastWritten = string.Empty;
                RAFFileListEntry RAFFile = x.Value[0];
                string n = Path.Combine(Client.ExecutingDirectory, "RADS", "lol_game_client");
                foreach (string directories in RAFFile.FileName.Split('/'))
                {
                    if (!directories.Contains('.'))
                    {
                        if (!Directory.Exists(Path.Combine(n, directories)))
                            Directory.CreateDirectory(Path.Combine(n, directories));

                        n = Path.Combine(n, directories);
                    }
                    else
                    {
                        try
                        {
                            var writer = new BinaryWriter(File.OpenWrite(Path.Combine(n, directories)));

                            // Writer raw data                
                            writer.Write(RAFFile.GetContent());
                            writer.Flush();
                            writer.Close();
                            FileLastWritten = Path.Combine(n, directories);
                        }
                        catch
                        {
                            LogTextBox("Unable to write " + Path.Combine(n, directories));
                        }
                    }
                }
                LogTextBox("(" + i + "/" + list.FileDictFull.Count + ") " +
                           ((i/(decimal) list.FileDictFull.Count)*100).ToString("N2") + "%");
                i += 1;
            }
            return true;
        }

        private void FindClient_Click(object sender, RoutedEventArgs e)
        {
            GetLolRootPath(true);
        }

        private void UpdateRegionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UpdateRegionComboBox.SelectedValue != null)
            {
                Settings.Default.updateRegion = (string)UpdateRegionComboBox.SelectedValue;
                ClientRegionLocation(Settings.Default.updateRegion);
                Settings.Default.Save();
            }
        }

        private void ClientRegionLocation(string regionName)
        {
            var findLeagueDialog = new OpenFileDialog();

            if (!Directory.Exists(Path.Combine("C:\\", "Riot Games", "League of Legends")))
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Program Files (x86)", "GarenaLoL", "GameData",
                    "Apps", "LoL");
            else
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Riot Games", "League of Legends");

            findLeagueDialog.DefaultExt = ".exe";
            findLeagueDialog.Filter = "League of Legends Launcher|lol.launcher*.exe|Garena Launcher|lol.exe";

            switch (regionName)
            {
                case "PBE": if (Settings.Default.PBELocation == string.Empty)
                    {
                        bool? result = findLeagueDialog.ShowDialog();
                        if (result != true)
                        {
                            Settings.Default.PBELocation = string.Empty;
                            break;
                        }
                        else Settings.Default.PBELocation = findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                    }
                    break;
                case "Live": if (Settings.Default.LiveLocation == string.Empty)
                    {
                        bool? result = findLeagueDialog.ShowDialog();
                        if (result != true)
                        {
                            Settings.Default.LiveLocation = string.Empty;
                            break;
                        }
                        else Settings.Default.LiveLocation = findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                    }
                    break;
                case "Korea": if (Settings.Default.KRLocation == string.Empty)
                    {
                        bool? result = findLeagueDialog.ShowDialog();
                        if (result != true)
                        {
                            Settings.Default.KRLocation = string.Empty;
                            break;
                        }
                        else Settings.Default.KRLocation = findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                    }
                    break;
                case "Garena": if (Settings.Default.GarenaLocation == string.Empty)
                    {
                        bool? result = findLeagueDialog.ShowDialog();
                        if (result != true)
                        {
                            Settings.Default.GarenaLocation = string.Empty;
                            break;
                        }
                        else Settings.Default.GarenaLocation = findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                    }
                    break;
                default:
                    break;
            }

        }
    }
}