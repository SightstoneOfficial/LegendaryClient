using LegendaryClient.Logic;
using LegendaryClient.Logic.Patcher;
using LegendaryClient.Logic.UpdateRegion;
using LegendaryClient.Properties;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.Win32;
using SharpCompress.Common;
using SharpCompress.Reader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for PatcherPage.xaml
    /// </summary>
    public partial class PatcherPage
    {
        private RiotPatcher patcher = new RiotPatcher();

        public PatcherPage()
        {
            InitializeComponent();
            UpdateRegionComboBox.SelectedValue = Settings.Default.updateRegion != string.Empty ? Settings.Default.updateRegion : "Live";
            Client.UpdateRegion = (string)UpdateRegionComboBox.SelectedValue;

            bool x = Settings.Default.DarkTheme;
            if (!x)
            {
                var bc = new BrushConverter();
                PatchTextBox.Background = (Brush)bc.ConvertFrom("#FFECECEC");
                DevKey.Background = (Brush)bc.ConvertFrom("#FFECECEC");
                PatchTextBox.Foreground = (Brush)bc.ConvertFrom("#FF1B1919");
                ExtractingProgressRing.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
            }

            autoPlayCheckBox.IsChecked = Settings.Default.AutoPlay;

            StartPatcher();
            Client.Log("LegendaryClient Started Up Successfully");
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
        private async void StartPatcher()
        {
            var client = new WebClient();
            client.DownloadProgressChanged +=
                (o, e) => Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    double bytesIn = double.Parse(e.BytesReceived.ToString(CultureInfo.InvariantCulture));
                    double totalBytes =
                        double.Parse(e.TotalBytesToReceive.ToString(CultureInfo.InvariantCulture));
                    double percentage = bytesIn / totalBytes * 100;

                    CurrentProgressLabel.Content = string.Format("Downloaded {0} MBs of {1} MBs",
                                                   (e.BytesReceived / 1024d / 1024d).ToString("0.00"),
                                                   (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
                    CurrentProgressBar.Value =
                        int.Parse(Math.Truncate(percentage).ToString(CultureInfo.InvariantCulture));
                }));

            await Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                TotalProgressLabel.Content = "20%";
                TotalProgessBar.Value = 20;
            }));
			#region Plugins
			LogTextBox("Checking Plugin dependencies...");
			//Check if LIB is not extracted
			if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Client", "LIB", "abc.py")))
			{
				LogTextBox("Plugin dependencies are installed");
			}
			else
			{
				if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Client", "LIB", "LIB.zip")))
				{
					LogTextBox("Extracting Plugin dependencies...");
					//Extract the zip
					ZipFile.ExtractToDirectory(Path.Combine(Client.ExecutingDirectory, "Client", "LIB", "LIB.zip"), Path.Combine(Client.ExecutingDirectory, "Client", "LIB"));
					//delete it
					File.Delete(Path.Combine(Client.ExecutingDirectory, "Client","LIB" , "LIB.zip"));
					LogTextBox("Plugin dependencies are installed");
				}
				else
				{
					LogTextBox("Plugin dependencies are NOT installed. Some features of the Plugins might not work!");
				}
			}
			#endregion

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


            string dDragonDownloadUrl = patcher.GetDragon();
            if (!string.IsNullOrEmpty(dDragonDownloadUrl))
            {
                LogTextBox("Newest DataDragon Version: " + patcher.DDragonVersion);
                string dDragonVersion =
                    File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_DDragon"));
                LogTextBox("Current DataDragon Version: " + dDragonVersion);

                Client.Log("DDragon Version (LOL Version) = " + dDragonVersion);

                if (patcher.DDragonVersion != dDragonVersion)
                {
                    try
                    {
                        if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "temp")))
                            Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "temp"));

                        await Dispatcher.BeginInvoke(DispatcherPriority.Input,
                            new ThreadStart(() => { CurrentProgressLabel.Content = "Downloading DataDragon"; }));

                        string ddragonLocation = Path.Combine(Client.ExecutingDirectory, "Assets",
                                "dragontail-" + patcher.DDragonVersion + ".tgz");
                        if (File.Exists(ddragonLocation))
                        {
                            client.OpenRead(dDragonDownloadUrl);
                            var asd = new FileInfo(ddragonLocation);
                            if (asd.Length != Convert.ToInt64(client.ResponseHeaders["Content-Length"]))
                            {
                                await client.DownloadFileTaskAsync(dDragonDownloadUrl,
                                    Path.Combine(Client.ExecutingDirectory, "Assets",
                                        "dragontail-" + patcher.DDragonVersion + ".tgz"));
                                await Task.Run(() => DDragonDownloaded());
                            }
                            else
                            {
                                await Task.Run(() => DDragonDownloaded());
                            }
                        }
                        else
                        {
                            await client.DownloadFileTaskAsync(dDragonDownloadUrl,
                                Path.Combine(Client.ExecutingDirectory, "Assets",
                                    "dragontail-" + patcher.DDragonVersion + ".tgz"));
                            await Task.Run(() => DDragonDownloaded());
                        }

                    }
                    catch
                    {
                        Client.Log(
                            "Probably updated version number without actually uploading the files.");
                    }
                }
                else
                {
                    AirPatcher();
                }
            }
            else
            {
                LogTextBox(
                    "Failed to get DDragon version. Either not able to be found or unknown error (most likely the website is in maitenance, please try again in an hour or so)");
                LogTextBox(
                    "Continuing could cause errors. Report this as an issue if it occurs again in a few hours.");
            }
        }

        private async void DDragonDownloaded()
        {
            await Dispatcher.BeginInvoke(DispatcherPriority.Input,
                new ThreadStart(() =>
                {
                    CurrentProgressLabel.Content = "Extracting DataDragon";
                    ExtractingLabel.Visibility = Visibility.Visible;
                    ExtractingProgressRing.Visibility = Visibility.Visible;
                }));

            Stream inStream =
                        File.OpenRead(Path.Combine(Client.ExecutingDirectory, "Assets",
                            "dragontail-" + patcher.DDragonVersion + ".tgz"));

            using (var reader = ReaderFactory.Open(inStream))
            {
                reader.WriteAllToDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "temp"),
                            ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                /*
                while (reader.MoveToNextEntry())
                {
                    if (!reader.Entry.IsDirectory)
                    {
                        reader.WriteAllToDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "temp"),
                            ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                    }
                }
                //*/
            }

            inStream.Close();
            Copy(
                Path.Combine(Client.ExecutingDirectory, "Assets", "temp", patcher.DDragonVersion,
                    "data"), Path.Combine(Client.ExecutingDirectory, "Assets", "data"));
            Copy(
                Path.Combine(Client.ExecutingDirectory, "Assets", "temp", patcher.DDragonVersion,
                    "img"), Path.Combine(Client.ExecutingDirectory, "Assets"));
            DeleteDirectoryRecursive(Path.Combine(Client.ExecutingDirectory, "Assets", "temp"));


            var encoding = new ASCIIEncoding();
            FileStream versionDDragon =
                File.Create(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_DDRagon"));
            versionDDragon.Write(encoding.GetBytes(patcher.DDragonVersion), 0,
                encoding.GetBytes(patcher.DDragonVersion).Length);

            versionDDragon.Close();
            await Dispatcher.BeginInvoke(DispatcherPriority.Input,
                new ThreadStart(() =>
                {
                    ExtractingLabel.Visibility = Visibility.Hidden;
                    ExtractingProgressRing.Visibility = Visibility.Hidden;
                }));
            AirPatcher();
        }
            #endregion DDragon

        private void AirPatcher()
        {
            try
            {
                var bgThead = new Thread(() =>
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        TotalProgressLabel.Content = "40%";
                        TotalProgessBar.Value = 40;
                    }));

                    // Try get LoL path from registry

                    //A string that looks like C:\Riot Games\League of Legends\
                    string lolRootPath = GetLolRootPath(false);

                    #region lol_air_client
                    var encoding = new ASCIIEncoding();
                    if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR")))
                    {
                        FileStream versionAir =
                            File.Create(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                        versionAir.Write(encoding.GetBytes("0.0.0.0"), 0, encoding.GetBytes("0.0.0.0").Length);
                        versionAir.Close();
                    }

                    BaseUpdateRegion updateRegion = BaseUpdateRegion.GetUpdateRegion(Client.UpdateRegion);
                    string latestAir = patcher.GetListing(updateRegion.AirListing);
                    LogTextBox("Newest Air Assets Version: " + latestAir);
                    string airVersion =
                        File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                    LogTextBox("Current Air Assets Version: " + airVersion);

                    downloadTheme(patcher.GetManifest(updateRegion.AirManifest + "releases/" + latestAir + "/packages/files/packagemanifest"));

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

                            updateClient.DownloadFile(new System.Uri(updateRegion.BaseLink + allFiles[i].Split(',')[0]), Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite"));

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

                    string solutionVersion = solutionListing.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                    Client.GameClientVersion = solutionVersion;
                    LogTextBox("Latest League of Legends GameClient: " + solutionVersion);
                    LogTextBox("Checking if League of Legends is Up-To-Date");

                    bool toExit = false;
                    if (Client.UpdateRegion == "Garena")
                    {
                        if (Settings.Default.GarenaLocation == string.Empty)
                            ClientRegionLocation("Garena");
                        if (File.Exists(Path.Combine(Settings.Default.GarenaLocation, "Air", "Lib", "ClientLibCommon.dat")))
                        {
                            File.Copy(Path.Combine(Settings.Default.GarenaLocation, "Air", "Lib", "ClientLibCommon.dat"),
                                      Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"), true);

                            LogTextBox("League of Legends is Up-To-Date");
                            Client.Location = Path.Combine(lolRootPath, "Game");
                            Client.RootLocation = lolRootPath;
                        }
                        else
                        {

                            LogTextBox("League of Legends is not Up-To-Date or location is incorrect");
                            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                            {
                                SkipPatchButton.IsEnabled = true;
                                FindClientButton.Visibility = Visibility.Visible;
                            }));
                            toExit = true;
                        }
                        /*
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
                        */
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
                        //SkipPatchButton_Click(null, null);

                        if (Settings.Default.AutoPlay)
                            SkipPatchButton_Click(null, null);
                    }));

                    LogTextBox("LegendaryClient Has Finished Patching");
                }) { IsBackground = true };

                bgThead.Start();
            }
            catch (Exception e)
            {
                Client.Log(e.Message + " - in PatcherPage updating progress.");
            }
        }

        private void downloadTheme(string[] manifest)
        {
            try
            {
                string[] fileMetaData = manifest.Skip(1).ToArray();
                BaseUpdateRegion updateRegion = BaseUpdateRegion.GetUpdateRegion(Client.UpdateRegion);

                if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "themes")))
                    Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "themes"));

                foreach (string s in fileMetaData)
                {
                    if (string.IsNullOrEmpty(s))
                        continue;

                    string location = s.Split(',')[0];
                    string savePlace = location.Split(new[] { "/files/" }, StringSplitOptions.None)[1];
                    if (savePlace.Contains("theme.properties"))
                    {
                        using (var newClient = new WebClient())
                        {
                            LogTextBox("Checking Theme...");
                            newClient.DownloadFile(updateRegion.BaseLink + location,
                                Path.Combine(Client.ExecutingDirectory, "Assets", "themes", "theme.properties"));
                        }
                    }
                }

                if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", "theme.properties")))
                    return;

                string[] file =
                    File.ReadAllLines(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", "theme.properties"));
                string theme = "";

                foreach (string s in file)
                    if (s.StartsWith("themeConfig="))
                        theme = s.Split('=')[1].Split(',')[0];

                if (theme == "")
                    return;

                if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme)))
                    Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme));
                else
                {
                    Client.Theme = theme;
                    return;
                }

                List<string> themeLink = fileMetaData.Where(
                                   line => (line.Contains("loop") || line.Contains("Loop")) &&
                                       line.Contains(theme)).ToList(); //loop is exacly the same as intro
                themeLink = themeLink.Select(link => link.Split(',')[0]).ToList();

                using (var newClient = new WebClient())
                {
                    foreach (var item in themeLink)
                    {
                        string fileName = item.Split('/').Last();
                        LogTextBox("Downloading " + fileName + " from http://l3cdn.riotgames.com");
                        newClient.DownloadFile(updateRegion.BaseLink + item,
                            Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme, fileName));

                    }
                }
                string[] flv = Directory.GetFiles(
                    Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme), "*.flv");

                foreach(var item in flv)
                {
                    var inputFile = new MediaFile { Filename = 
                        Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme, item) };
                    var outputFile = new MediaFile { Filename = 
                        Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme, item).Replace(".flv", ".mp4") };

                    using (var engine = new Engine())
                    {
                        engine.Convert(inputFile, outputFile);
                    }

                }
                Client.Theme = theme;
            }
            catch
            {
            }
        }

        private string GetLolRootPath(bool restart)
        {
            if (!restart)
            {
                switch (Client.UpdateRegion)
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
                    case "Garena": if (Settings.Default.GarenaLocation != string.Empty && Settings.Default.GarenaLocation.EndsWith("lol.exe"))
                        {
                            Settings.Default.GarenaLocation = Settings.Default.GarenaLocation.Replace("lol.exe", "");
                            return Settings.Default.GarenaLocation;
                        }
                        else if (Settings.Default.GarenaLocation != string.Empty)
                            return Settings.Default.GarenaLocation;
                        else break;
                    default:
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
                    catch
                    {
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

        private void LogTextBox(string s)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                PatchTextBox.Text += "[" + DateTime.Now.ToShortTimeString() + "] " + s + Environment.NewLine;
                PatchTextBox.ScrollToEnd();
                this.Focus();
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
                if (string.IsNullOrEmpty(s))
                    continue;

                //Remove size and type metadata
                string location = s.Split(',')[0];
                //Get save position
                var version = new Version(location.Split(new[] { "/releases/", "/files/" }, StringSplitOptions.None)[1]);
                if (version <= currentVersion && currentVersion != new Version("0.0.0.0"))
                    continue;
                BaseUpdateRegion updateRegion = BaseUpdateRegion.GetUpdateRegion(Client.UpdateRegion);

                string savePlace = location.Split(new[] { "/files/" }, StringSplitOptions.None)[1];
                if (!savePlace.EndsWith(".jpg") && !savePlace.EndsWith(".png") && !savePlace.EndsWith(".mp3"))
                    continue;

                if (savePlace.Contains("assets/images/champions/"))
                {
                    using (var newClient = new WebClient())
                    {
                        string saveName = location.Split(new[] { "/champions/" }, StringSplitOptions.None)[1];
                        LogTextBox("Downloading " + saveName + " from http://l3cdn.riotgames.com");
                        newClient.DownloadFile(updateRegion.BaseLink + location,
                            Path.Combine(Client.ExecutingDirectory, "Assets", "champions", saveName));
                    }
                }
                else if (savePlace.Contains("assets/images/abilities/"))
                {
                    using (var newClient = new WebClient())
                    {
                        string saveName = location.Split(new[] { "/abilities/" }, StringSplitOptions.None)[1];
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
                                ? Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", saveName.Split(new[] { "_" }, StringSplitOptions.None)[0] + ".png")
                                : Path.Combine(Client.ExecutingDirectory, "Assets", "profileicon", saveName.Replace("profileIcon", string.Empty)));
                    }
                }
                else if (savePlace.Contains("assets/sounds/"))
                {
                    using (var newClient = new WebClient())
                    {
                        if (savePlace.Contains("en_US/champions/"))
                        {
                            string saveName = location.Split(new[] { "/champions/" }, StringSplitOptions.None)[1];
                            LogTextBox("Downloading " + saveName + " from http://l3cdn.riotgames.com");
                            newClient.DownloadFile(updateRegion.BaseLink + location,
                                Path.Combine(Client.ExecutingDirectory, "Assets", "sounds", "champions",
                                    saveName));
                        }
                        else if (savePlace.Contains("assets/sounds/ambient"))
                        {
                            string saveName = location.Split(new[] { "/ambient/" }, StringSplitOptions.None)[1];
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
                    var result = findLeagueDialog.ShowDialog();
                    Settings.Default.PBELocation = result != true ? string.Empty : findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                }
                    break;
                case "Live": if (Settings.Default.LiveLocation == string.Empty)
                {
                    var result = findLeagueDialog.ShowDialog();
                    Settings.Default.LiveLocation = result != true ? string.Empty : findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                }
                    break;
                case "Korea": if (Settings.Default.KRLocation == string.Empty)
                {
                    var result = findLeagueDialog.ShowDialog();
                    Settings.Default.KRLocation = result != true ? string.Empty : findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                }
                    break;
                case "Garena": if (Settings.Default.GarenaLocation == string.Empty)
                {
                    var result = findLeagueDialog.ShowDialog();
                    Settings.Default.GarenaLocation = result != true ? string.Empty : findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "").Replace("lol.exe","");
                }
                    break;
            }

        }

        private void AutoPlay_Changed(object sender, RoutedEventArgs e)
        {
            Settings.Default.AutoPlay = autoPlayCheckBox.IsChecked.Value;
        }
    }
}
