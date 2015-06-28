using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Riot;
using LegendaryClient.Logic.SWF;
using LegendaryClient.Logic.SWF.SWFTypes;
using LegendaryClient.Logic.UpdateRegion;
using LegendaryClient.Properties;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using SharpCompress.Common;
using SharpCompress.Reader;

namespace LegendaryClient.Windows
{
    /// <summary>
    ///     Interaction logic for PatcherPage.xaml
    /// </summary>
    public partial class PatcherPage
    {
        public PatcherPage()
        {
            InitializeComponent();

            UpdateRegionComboBox.SelectedValue = Settings.Default.updateRegion != string.Empty
                ? Settings.Default.updateRegion
                : "Live";
            Client.UpdateRegion = (string) UpdateRegionComboBox.SelectedValue;

            var x = Settings.Default.DarkTheme;
            if (!x)
            {
                var bc = new BrushConverter();
                PatchTextBox.Background = (Brush) bc.ConvertFrom("#FFECECEC");
                DevKey.Background = (Brush) bc.ConvertFrom("#FFECECEC");
                PatchTextBox.Foreground = (Brush) bc.ConvertFrom("#FF1B1919");
                ExtractingProgressRing.Foreground = (Brush) bc.ConvertFrom("#FFFFFFFF");
            }
            autoPlayCheckBox.IsChecked = Settings.Default.AutoPlay;

            StartPatcher();
            Client.Log("LegendaryClient Started Up Successfully");
        }

        private async void StartPatcher()
        {
            var patcherClient = new WebClient();
            patcherClient.DownloadProgressChanged += WebClientOnDownloadProgressChanged;

            TotalProgessBar.Value = 10;
            LogTextBox("Updating Plugin Dependencies");
            CheckPluginDependencies();

            TotalProgessBar.Value = 40;
            LogTextBox("");
            LogTextBox("Updating Data Dragon");
            await DownloadDataDragon(patcherClient);

            TotalProgessBar.Value = 80;
            LogTextBox("");
            LogTextBox("Updating Air Assets");
            await PatchAir();

            LogTextBox("");
            LogTextBox("Checking Game Version");
            if (UpdateGameClient())
                return;

            TotalProgessBar.Value = 100;
            SkipPatchButton.Content = "Play";
            CurrentProgressLabel.Content = "Finished Patching";
            CurrentStatusLabel.Content = "Ready To Play";
            SkipPatchButton.IsEnabled = true;
            //SkipPatchButton_Click(null, null);

            if (Settings.Default.AutoPlay)
                SkipPatchButton_Click(null, null);

            LogTextBox("");
            LogTextBox("LegendaryClient Has Finished Patching");
        }
 
        #region Helper Functions

        private void WebClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            var percentage = Convert.ToDouble(e.BytesReceived) / Convert.ToDouble(e.TotalBytesToReceive) * 100d;

            CurrentProgressLabel.Content = String.Format("Downloaded {0:0.00} MBs of {1:0.00} MBs", e.BytesReceived / 1024d / 1024d, e.TotalBytesToReceive / 1024d / 1024d);
            CurrentProgressBar.Value = percentage;
        }


        private static string GetHumanSize(long totalSize)
        {
            double val = totalSize;
            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;
            while (val >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                val = val / 1024d;
            }

            string humanSize = String.Format("{0:0.##} {1}", val, sizes[order]);
            return humanSize;
        }

        private static string GetAssetsDirectory(params string[] sub)
        {
            var parts = new List<string> {Client.ExecutingDirectory, "Assets"};
            parts.AddRange(sub);
            return Path.Combine(parts.ToArray());
        }

        private string GetLolRootPath(bool restart)
        {
            if (!restart)
            {
                switch (Client.UpdateRegion)
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
                    new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\RIOT GAMES", "Path"),
                    new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\RIOT GAMES\RADS", "LocalRootFolder"),
                    new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\Wow6432Node\RIOT GAMES", "Path"),
                    new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\Wow6432Node\RIOT GAMES\RADS", "LocalRootFolder"),
                    new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Riot Games\RADS", "LocalRootFolder"),
                    new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games\RADS", "LocalRootFolder")
                };

                foreach (var tuple in possiblePaths)
                {
                    var path = tuple.Item1;
                    var valueName = tuple.Item2;
                    try
                    {
                        var value = Registry.GetValue(path, valueName, string.Empty);
                        if (value != null && value.ToString() != string.Empty)
                        {
                            var regKey = Registry.CurrentUser.CreateSubKey("LegendaryClient");
                            if (regKey != null)
                            {
                                regKey.SetValue(value.ToString().Contains("lol.exe")
                                    ? "GarenaLocation"
                                    : "LoLLocation",
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
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Program Files (x86)", "GarenaLoL", "GameData", "Apps", "LoL");
            else
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Riot Games", "League of Legends");

            findLeagueDialog.DefaultExt = ".exe";
            findLeagueDialog.Filter = "League of Legends Launcher|lol.launcher*.exe|Garena Launcher|lol.exe";

            var result = findLeagueDialog.ShowDialog();
            if (result != true)
                return string.Empty;

            var key = Registry.CurrentUser.CreateSubKey("Software\\RIOT GAMES");
            if (key != null)
                key.SetValue("Path", findLeagueDialog.FileName.Replace("lol.launcher.exe", string.Empty).Replace("lol.launcher.admin.exe", string.Empty));

            if (restart)
                LogTextBox("Saved value, please restart the client to login.");

            return findLeagueDialog.FileName.Replace("lol.launcher.exe", string.Empty).Replace("lol.launcher.admin.exe", string.Empty);
        }
        
        private void LogTextBox(string s)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input,
                new ThreadStart(() =>
                {
                    if (String.IsNullOrEmpty(s))
                        PatchTextBox.Text += Environment.NewLine;
                    else
                    {
                        PatchTextBox.Text += "[" + DateTime.Now.ToShortTimeString() + "] " + s + Environment.NewLine;
                        PatchTextBox.ScrollToEnd();
                        Focus();
                    }
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

        private void DeleteDirectoryRecursive(string path)
        {
            foreach (var directory in Directory.GetDirectories(path))
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

        #endregion

        #region DataDragon

        public string DDragonVersion;

        public string GetDragonUrl()
        {
            var dragonJson = String.Empty;
            using (var client = new WebClient())
            {
                try
                {
                    dragonJson = client.DownloadString("http://ddragon.leagueoflegends.com/realms/na.json");
                }
                catch (Exception)
                {
                }
            }

            if (string.IsNullOrEmpty(dragonJson))
                return string.Empty;

            var deserializedJson = JObject.Parse(dragonJson);
            var version = deserializedJson.GetValue("v").Value<string>();
            var cdn = deserializedJson.GetValue("cdn").Value<string>();
            var s = cdn + "/dragontail-" + version + ".tgz";
            DDragonVersion = version;

            return s;
        }

        private async Task DownloadDataDragon(WebClient patcherClient)
        {
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets"));

            if (!File.Exists(GetAssetsDirectory("VERSION_DDRagon")))
            {
                File.WriteAllText(GetAssetsDirectory("VERSION_DDRagon"), "0.0.0");
            }


            var dDragonDownloadUrl = GetDragonUrl();
            if (String.IsNullOrEmpty(dDragonDownloadUrl))
            {
                LogTextBox("Failed to get DDragon version. Either not able to be found or unknown error (most likely the website is in maitenance, please try again in an hour or so)");
                LogTextBox("Continuing could cause errors. Report this as an issue if it occurs again in a few hours.");
                return;
            }

            var dDragonVersion = File.ReadAllText(GetAssetsDirectory("VERSION_DDragon"));
            LogTextBox("Newest DataDragon Version: " + DDragonVersion);
            LogTextBox("Current DataDragon Version: " + dDragonVersion);

            if (DDragonVersion != dDragonVersion)
            {
                LogTextBox("Downloading DataDragon");
                try
                {
                    if (!Directory.Exists(GetAssetsDirectory("temp")))
                        Directory.CreateDirectory(GetAssetsDirectory("temp"));

                    CurrentProgressLabel.Content = "Downloading DataDragon";

                    var ddragonLocation = Path.Combine(GetAssetsDirectory("dragontail-" + DDragonVersion + ".tgz"));
                    if (File.Exists(ddragonLocation))
                    {
                        File.Delete(ddragonLocation);
                    }
                    await patcherClient.DownloadFileTaskAsync(dDragonDownloadUrl, GetAssetsDirectory("dragontail-" + DDragonVersion + ".tgz"));
                    await ExtractDDragon();
                }
                catch
                {
                    Client.Log("Probably updated version number without actually downloading the files.");
                }
            }
        }

        private async Task ExtractDDragon()
        {
            CurrentProgressLabel.Content = "Extracting DataDragon";
            ExtractingLabel.Visibility = Visibility.Visible;
            ExtractingProgressRing.Visibility = Visibility.Visible;

            await Task.Run(() =>
            {
                Stream inStream = File.OpenRead(GetAssetsDirectory("dragontail-" + DDragonVersion + ".tgz"));
                using (var reader = ReaderFactory.Open(inStream))
                {
                    reader.WriteAllToDirectory(GetAssetsDirectory("temp"), ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                }
                inStream.Close();
                Copy(Path.Combine(GetAssetsDirectory("temp"), DDragonVersion, "data"), GetAssetsDirectory("data"));
                Copy(Path.Combine(GetAssetsDirectory("temp"), DDragonVersion, "img"), GetAssetsDirectory(""));
            });

            //We don't need to wait till all files are deleted
#pragma warning disable 4014
            Task.Run(() => DeleteDirectoryRecursive(GetAssetsDirectory("temp")));
#pragma warning restore 4014

            File.WriteAllText(GetAssetsDirectory("VERSION_DDRagon"), DDragonVersion);

            ExtractingLabel.Visibility = Visibility.Hidden;
            ExtractingProgressRing.Visibility = Visibility.Hidden;
        }

        #endregion

        #region Plugins

        private void CheckPluginDependencies()
        {
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
                    ZipFile.ExtractToDirectory(Path.Combine(Client.ExecutingDirectory, "Client", "LIB", "LIB.zip"), Path.Combine(Client.ExecutingDirectory, "Client", "LIB"));
                    File.Delete(Path.Combine(Client.ExecutingDirectory, "Client", "LIB", "LIB.zip"));
                    LogTextBox("Plugin dependencies are installed");
                }
                else
                {
                    LogTextBox("Plugin dependencies are NOT installed. Some features of the Plugins might not work!");
                }
            }
        }

        #endregion

        #region GameClient

        private bool UpdateGameClient()
        {
            var updateRegion = BaseUpdateRegion.GetUpdateRegion(Client.UpdateRegion);
            var lolRootPath = GetLolRootPath(false);

            LogTextBox("Trying to detect League of Legends GameClient");
            LogTextBox("League of Legends is located at: " + lolRootPath);

            //RADS\solutions\lol_game_client_sln\releases
            string gameLocation = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln", "releases");

            string[] solutionReleases = GetGameClientReleases(updateRegion.SolutionListing);
            string latestSolutionVersion = solutionReleases.First();

            Client.GameClientVersion = latestSolutionVersion;
            LogTextBox("Latest League of Legends GameClient: " + latestSolutionVersion);

            bool toExit = false;
            if (Client.UpdateRegion == "Garena")
            {
                if (Settings.Default.GarenaLocation == string.Empty)
                    ClientRegionLocation("Garena");
                if (File.Exists(Path.Combine(Settings.Default.GarenaLocation, "Air", "Lib", "ClientLibCommon.dat")))
                {
                    File.Copy(Path.Combine(Settings.Default.GarenaLocation, "Air", "Lib", "ClientLibCommon.dat"),
                        Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"),
                        true);

                    LogTextBox("League of Legends is Up-To-Date");
                    Client.Location = Path.Combine(lolRootPath, "Game");
                    Client.RootLocation = lolRootPath;
                }
                else
                {
                    LogTextBox("League of Legends is not Up-To-Date or location is incorrect");
                    Dispatcher.BeginInvoke(DispatcherPriority.Input,
                        new ThreadStart(() =>
                        {
                            SkipPatchButton.IsEnabled = true;
                            FindClientButton.Visibility = Visibility.Visible;
                        }));
                    toExit = true;
                }
            }
            else if (Directory.Exists(Path.Combine(gameLocation, latestSolutionVersion)))
            {
                LogTextBox("League of Legends is Up-To-Date");
                Client.Location = Path.Combine(lolRootPath, "RADS", "solutions", "lol_game_client_sln", "releases", latestSolutionVersion, "deploy");
                Client.RootLocation = lolRootPath;
            }
            else
            {
                LogTextBox("League of Legends is not Up-To-Date or location is incorrect. Please Update League Of Legends");
                SkipPatchButton.IsEnabled = true;
                FindClientButton.Visibility = Visibility.Visible;
                toExit = true;
            }

            if (toExit)
                return true;
            return false;
        }

        public string[] GetGameClientReleases(string listingLink)
        {
            using (var client = new WebClient())
            {
                try
                {
                    var versions = client.DownloadString(listingLink);
                    return Regex.Split(versions, Environment.NewLine);
                }
                catch (WebException e)
                {
                    Client.Log(e.Message);
                }
            }
            throw new Exception("Unable to fetch Releaselist");
        }

        #endregion

        #region Air

        private async Task PatchAir()
        {
            try
            {
                if (!File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR")))
                {
                    File.WriteAllText(GetAssetsDirectory("VERSION_AIR"), "0.0.0.0");
                }

                //Anything is based on NA. EUW and other regions may have different files and so on
                var updateRegion = BaseUpdateRegion.GetUpdateRegion(Client.UpdateRegion);

                var airVersions = GetAirReleases(updateRegion.AirListing);
                var latestAirVersion = airVersions.FirstOrDefault();
                var currentAirVersion = File.ReadAllText(GetAssetsDirectory("VERSION_AIR"));

                LogTextBox("Newest Air Assets Version: " + latestAirVersion);
                LogTextBox("Current Air Assets Version: " + currentAirVersion);

                var manifestList = await GetManifest(updateRegion, latestAirVersion);
                await DownloadTheme(manifestList);

                var updateClient = new WebClient();
                if (currentAirVersion != latestAirVersion)
                {
                    CurrentProgressLabel.Content = "Retrieving Air Assets";
                    try
                    {
                        LogTextBox(String.Format("Downloading {0} ({1:00} KB)from http://l3cdn.riotgames.com",
                            "gameStats_en_US.sqlite",
                            manifestList.First(m => m.RelativePath.Contains("gameStats_en_US.sqlite")).FileSize));
                        await updateClient.DownloadFileTaskAsync(manifestList.First(m => m.RelativePath.Contains("gameStats_en_US.sqlite")).AbsolutePath,
                                Path.Combine(Client.ExecutingDirectory, "gameStats_en_US.sqlite"));


                        if (Client.UpdateRegion != "Garena")
                        {
                            LogTextBox(String.Format("Downloading {0} ({1:00} KB)from http://l3cdn.riotgames.com",
                                "ClientLibCommon.dat",
                                manifestList.First(m => m.RelativePath.Contains("ClientLibCommon.dat")).FileSize));
                            await updateClient.DownloadFileTaskAsync(
                                manifestList.First(m => m.RelativePath.Contains("ClientLibCommon.dat")).AbsolutePath, Path.Combine(Client.ExecutingDirectory, "ClientLibCommon.dat"));
                        }

                        await GetMediaFiles(manifestList);
                        if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR")))
                        {
                            File.Delete(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                        }
                        File.WriteAllText(GetAssetsDirectory("VERSION_AIR"), latestAirVersion);
                    }
                    catch (Exception e)
                    {
                        Client.Log(e.Message);
                    }
                    SkipPatchButton.IsEnabled = true;
                }
            }
            catch (Exception e)
            {
                Client.Log(e.Message + " - in PatcherPage updating progress.");
            }
        }

        public string[] GetAirReleases(string listingLink)
        {
            using (var client = new WebClient())
            {
                try
                {
                    var versions = client.DownloadString(listingLink);
                    return Regex.Split(versions, Environment.NewLine);
                }
                catch (WebException e)
                {
                    Client.Log(e.Message);
                }
            }
            throw new Exception("Unable to fetch Releaselist");
        }

        public async Task<PatcherFile[]> GetManifest(BaseUpdateRegion updateRegion, string latestAirVersion)
        {
            using (var client = new WebClient())
            {
                try
                {
                    var manifestLink = updateRegion.AirManifest + "releases/" + latestAirVersion + "/packages/files/packagemanifest";
                    var manifestString = await client.DownloadStringTaskAsync(manifestLink);
                    var manifestLines = Regex.Split(manifestString, Environment.NewLine);
                    var files = manifestLines.Where(m => m.Contains(',')).Select(m => new PatcherFile(updateRegion, m));
                    return files.ToArray();
                }
                catch (WebException e)
                {
                    Client.Log(e.Message);
                }
            }
            throw new Exception("Unable to fetch Packagemanifest");
        }

        private async Task DownloadTheme(PatcherFile[] files)
        {
            try
            {
                if (!Directory.Exists(GetAssetsDirectory("themes")))
                    Directory.CreateDirectory(GetAssetsDirectory("themes"));

                using (var wc = new WebClient())
                {
                    LogTextBox("Updating Theme");
                    await wc.DownloadFileTaskAsync(files.First(f => f.RelativePath.Contains("theme.properties")).AbsolutePath, GetAssetsDirectory("themes", "theme.properties"));
                }

                if (!File.Exists(GetAssetsDirectory("themes", "theme.properties")))
                    throw new Exception("Downloading of the file \"theme.properties\" failed");

                var theme = File.ReadAllLines(GetAssetsDirectory("themes", "theme.properties")).First(s => s.StartsWith("themeConfig=")).Split('=')[1].Split(',')[0];
                if (theme == "")
                    return;

                if (!Directory.Exists(GetAssetsDirectory("themes", theme)))
                    Directory.CreateDirectory(GetAssetsDirectory("themes", theme));
                else if (Directory.GetFiles(GetAssetsDirectory("themes", theme)).Any())
                {
                    Client.Theme = theme;
                    return;
                }

                var themePatcherFiles = files.Where(l => l.RelativePath.ToLower().Contains("loop") && l.RelativePath.Contains(theme)).ToList();
                Parallel.ForEach(themePatcherFiles,
                    new ParallelOptions {MaxDegreeOfParallelism = 10},
                    file =>
                    {
                        LogTextBox(String.Format("Downloading {0} ({1} KB) from http://l3cdn.riotgames.com", Path.GetFileName(file.RelativePath), file.FileSize/1024));
                        var fname = Path.GetFileName(file.RelativePath);
                        var localPath = GetAssetsDirectory("themes", theme, fname);
                        new WebClient().DownloadFile(file.AbsolutePath, localPath);
                    });

                var flv = Directory.GetFiles(GetAssetsDirectory("themes", theme), "*.flv");

                foreach (var item in flv)
                {
                    var inputFile = new MediaFile {Filename = GetAssetsDirectory("themes", theme, item)};

                    var outputFile = new MediaFile {Filename = GetAssetsDirectory("themes", theme, item).Replace(".flv", ".mp4")};

                    LogTextBox(String.Format("Converting {0}", Path.GetFileName(item)));
                    await Task.Run(() =>
                    {
                        using (var engine = new Engine())
                            engine.Convert(inputFile, outputFile);
                    });
                }
                Client.Theme = theme;
            }
            catch (Exception ex)
            {
                Client.Log(ex);
            }
        }

        private async Task GetMediaFiles(PatcherFile[] files)
        {
            var currentVersion = new Version(File.ReadAllText(GetAssetsDirectory("VERSION_AIR")));

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

            //Select all files to calculate the size
            files =
                files.Where(
                    f =>
                        new Version(f.Version) > currentVersion && (f.SavePath.EndsWith(".jpg") || f.SavePath.EndsWith(".png") || f.SavePath.EndsWith(".mp3")) &&
                        (f.SavePath.Contains("assets/images/champions/") || f.SavePath.Contains("assets/images/abilities/") ||
                         f.SavePath.Contains("assets/storeImages/content/summoner_icon/") || (f.SavePath.Contains("assets/sounds/") && f.SavePath.Contains("en_US/champions/")) ||
                         (f.SavePath.Contains("assets/sounds/") && f.SavePath.Contains("assets/sounds/ambient")) ||
                         (f.SavePath.Contains("assets/sounds/") && f.SavePath.Contains("assets/sounds/matchmakingqueued.mp3")))).ToArray();

            _mediaTotalSize = files.Sum(s => s.FileSize);

            var humanSize = GetHumanSize(_mediaTotalSize);
            LogTextBox(String.Format("Downloading {0} files ({1}) from http://l3cdn.riotgames.com", files.Length, humanSize));

            await Task.Run(() => Parallel.ForEach(files, new ParallelOptions() {MaxDegreeOfParallelism = 10}, DoFileDownload));
        }

        private long _mediaTotalSize;
        private long _mediaDownloaded;
        private readonly object _mediaDownloadedLock = new object();

        private void DoFileDownload(PatcherFile f)
        {
            var savePlace = f.SavePath;

            if (savePlace.Contains("assets/images/champions/"))
            {
                using (var wc = new WebClient())
                {
                    var saveName = Regex.Split(f.SavePath, "/champions/")[1];
                    //LogTextBox(String.Format("Downloading {0} ({1}KB) from http://l3cdn.riotgames.com", Path.GetFileName(f.RelativePath), f.FileSize/1024));
                    wc.DownloadFile(f.AbsolutePath, GetAssetsDirectory("champions", saveName));
                    lock (_mediaDownloadedLock)
                        _mediaDownloaded += f.FileSize;
                }
            }
            else if (savePlace.Contains("assets/images/abilities/"))
            {
                using (var wc = new WebClient())
                {
                    var saveName = Regex.Split(f.SavePath, "/abilities/")[1];
                    //LogTextBox(String.Format("Downloading {0} ({1}KB) from http://l3cdn.riotgames.com", Path.GetFileName(f.RelativePath), f.FileSize/1024));
                    wc.DownloadFile(f.AbsolutePath,
                        saveName.ToLower().Contains("passive")
                            ? GetAssetsDirectory("passive", saveName)
                            : GetAssetsDirectory("spell", saveName));
                    lock (_mediaDownloadedLock)
                        _mediaDownloaded += f.FileSize;
                }
            }
            else if (savePlace.Contains("assets/storeImages/content/summoner_icon/"))
            {
                using (var wc = new WebClient())
                {
                    var saveName = Regex.Split(f.SavePath, "/summoner_icon/")[1];
                    //LogTextBox(String.Format("Downloading {0} ({1}KB) from http://l3cdn.riotgames.com", Path.GetFileName(f.RelativePath), f.FileSize/1024));
                    wc.DownloadFile(f.AbsolutePath,
                        saveName.ToLower().Contains("_")
                            ? GetAssetsDirectory("profileicon", Regex.Split(saveName, "_")[0] + ".png")
                            : GetAssetsDirectory("profileicon", saveName.Replace("profileIcon", String.Empty)));
                    lock (_mediaDownloadedLock)
                        _mediaDownloaded += f.FileSize;
                }
            }
            else if (savePlace.Contains("assets/sounds/"))
            {
                using (var wc = new WebClient())
                {
                    if (savePlace.Contains("en_US/champions/"))
                    {
                        var saveName = Regex.Split(f.SavePath, "/champions/")[1];
                        //LogTextBox(String.Format("Downloading {0} ({1}KB) from http://l3cdn.riotgames.com", Path.GetFileName(f.RelativePath), f.FileSize/1024));
                        wc.DownloadFile(f.AbsolutePath, GetAssetsDirectory("sounds", "champions", saveName));
                        lock (_mediaDownloadedLock)
                            _mediaDownloaded += f.FileSize;
                    }
                    else if (savePlace.Contains("assets/sounds/ambient"))
                    {
                        var saveName = Regex.Split(f.SavePath, "/ambient/")[1];
                        //LogTextBox(String.Format("Downloading {0} ({1}KB) from http://l3cdn.riotgames.com", Path.GetFileName(f.RelativePath), f.FileSize/1024));
                        wc.DownloadFile(f.AbsolutePath, GetAssetsDirectory("sounds", "ambient", saveName));
                        lock (_mediaDownloadedLock)
                            _mediaDownloaded += f.FileSize;
                    }
                    else if (savePlace.Contains("assets/sounds/matchmakingqueued.mp3"))
                    {
                        //LogTextBox(String.Format("Downloading {0} ({1}KB) from http://l3cdn.riotgames.com", Path.GetFileName(f.RelativePath), f.FileSize/1024));
                        wc.DownloadFile(f.AbsolutePath, GetAssetsDirectory("sounds", "matchmakingqueued.mp3"));
                        lock (_mediaDownloadedLock)
                            _mediaDownloaded += f.FileSize;
                    }
                    else
                        Debugger.Break();
                }
            }
            else
                Debugger.Break();

            Dispatcher.Invoke(() =>
            {
                double percentage;
                long finished;

                lock (_mediaDownloadedLock)
                {
                    percentage = Convert.ToDouble(_mediaDownloaded)/Convert.ToDouble(_mediaTotalSize)*100d;
                    finished = _mediaDownloaded;
                }

                string humanTotal = GetHumanSize(_mediaTotalSize);
                string humanFinished = GetHumanSize(finished);

                CurrentProgressLabel.Content = String.Format("Downloaded {0} of {1}", humanFinished, humanTotal);
                CurrentProgressBar.Value = percentage;
            });
        }

        #endregion

        #region UI Management

        private void DevSkip_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new LoginPage());
            Client.Log("Swiched to LoginPage with DevSkip");
        }

        private void SkipPatchButton_Click(object sender, RoutedEventArgs e)
        {
            Client.SwitchPage(new LoginPage());
        }

        private void FindClient_Click(object sender, RoutedEventArgs e)
        {
            GetLolRootPath(true);
        }

        private void UpdateRegionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UpdateRegionComboBox.SelectedValue != null)
            {
                Settings.Default.updateRegion = (string) UpdateRegionComboBox.SelectedValue;
                ClientRegionLocation(Settings.Default.updateRegion);
                Settings.Default.Save();

                var warn = new Warning();
                warn.Header.Content = "Restart";
                warn.MessageText.Text = "Please restart LegendaryClient to apply the changes!";

                warn.ReturnButton.Content = "Do not close";
                warn.ReturnButton.Click += (o, args) => Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;

                warn.ExitButton.Content = "Close";
                warn.ExitButton.Click += (o, args) => Environment.Exit(0);

                warn.HideButton.Click += (o, args) => Client.FullNotificationOverlayContainer.Visibility = Visibility.Hidden;

                Client.FullNotificationOverlayContainer.Content = warn.Content;
                Client.FullNotificationOverlayContainer.Visibility = Visibility.Visible;
            }
        }

        private void ClientRegionLocation(string regionName)
        {
            var findLeagueDialog = new OpenFileDialog();

            if (!Directory.Exists(Path.Combine("C:\\", "Riot Games", "League of Legends")))
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Program Files (x86)", "GarenaLoL", "GameData", "Apps", "LoL");
            else
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Riot Games", "League of Legends");

            findLeagueDialog.DefaultExt = ".exe";
            findLeagueDialog.Filter = "League of Legends Launcher|lol.launcher*.exe|Garena Launcher|lol.exe";

            switch (regionName)
            {
                case "PBE":
                    if (Settings.Default.PBELocation == string.Empty)
                    {
                        var result = findLeagueDialog.ShowDialog();
                        Settings.Default.PBELocation = result != true
                            ? string.Empty
                            : findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                    }
                    break;
                case "Live":
                    if (Settings.Default.LiveLocation == string.Empty)
                    {
                        var result = findLeagueDialog.ShowDialog();
                        Settings.Default.LiveLocation = result != true
                            ? string.Empty
                            : findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                    }
                    break;
                case "Korea":
                    if (Settings.Default.KRLocation == string.Empty)
                    {
                        var result = findLeagueDialog.ShowDialog();
                        Settings.Default.KRLocation = result != true
                            ? string.Empty
                            : findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "");
                    }
                    break;
                case "Garena":
                    if (Settings.Default.GarenaLocation == string.Empty)
                    {
                        var result = findLeagueDialog.ShowDialog();
                        Settings.Default.GarenaLocation = result != true
                            ? string.Empty
                            : findLeagueDialog.FileName.Replace("lol.launcher.admin.exe", "").Replace("lol.launcher.exe", "").Replace("lol.exe", "");
                    }
                    break;
            }
        }

        private void AutoPlay_Changed(object sender, RoutedEventArgs e)
        {
            Settings.Default.AutoPlay = autoPlayCheckBox.IsChecked == true;
        }

        private void TotalProgessBar_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TotalProgressLabel.Content = e.NewValue.ToString("N0") + "%";
        }

        #endregion
    }

    public class PatcherFile
    {
        public PatcherFile(BaseUpdateRegion r, string path, string version, long size)
        {
            RelativePath = path;
            Version = version;
            FileSize = size;
            Region = r;
        }

        public PatcherFile(BaseUpdateRegion r, string path, string version, string size) : this(r, path, version, Convert.ToInt64(size))
        {
        }

        public PatcherFile(BaseUpdateRegion r, string path, string size) : this(r, path, path.Substring(34).Substring(0, path.Substring(34).IndexOf('/')), size)
        {
        }

        public PatcherFile(BaseUpdateRegion r, string line) : this(r, line.Split(',')[0], line.Split(',')[3])
        {
        }

        public BaseUpdateRegion Region { get; set; }
        public string RelativePath { get; set; }
        public string Version { get; set; }
        public long FileSize { get; set; }

        public string AbsolutePath
        {
            get { return String.Format("{0}{1}", Region.BaseLink, RelativePath); }
        }

        public string SavePath
        {
            get { return Regex.Split(RelativePath, "/files/")[1]; }
        }
    }
}