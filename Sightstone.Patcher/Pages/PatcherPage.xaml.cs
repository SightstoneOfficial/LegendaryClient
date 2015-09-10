#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Sightstone.Patcher.Logic;
using Sightstone.Patcher.PatcherElements;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.Win32;
using Sightstone.Logic.UpdateRegion;
using Sightstone.Patcher.Logic.Region;
using Brush = System.Windows.Media.Brush;
using Image = System.Drawing.Image;
using Sightstone.Patcher.Properties;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Shell;

#endregion

namespace Sightstone.Patcher.Pages
{
    /// <summary>
    ///     Interaction logic for PatcherPage.xaml
    ///     Future patcher that will make it so users will no longer need League of Legends on their PCs
    ///     Updates SplashUpdate as well just incase it is needed
    /// </summary>
    public partial class PatcherPage
    {
        private readonly string _executingDirectory;
        public bool IsLogVisible;
        public bool DownloadStarted;

        /// <summary>
        ///     List all SWF image packs which were updated
        /// </summary>
        public List<updateSWF> SWFextract;

        public PatcherPage()
        {
            InitializeComponent();
            _executingDirectory = Client.ExecutingDirectory;
            IsLogVisible = false;

            _executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomainFirstChanceException;
            if (_executingDirectory != null &&
                File.Exists(Path.Combine(_executingDirectory, "SightstonePatcher.log")))
            {
                File.Delete(Path.Combine(_executingDirectory, "SightstonePatcher.log"));
                File.Create(Path.Combine(_executingDirectory, "SightstonePatcher.log"));
            }
            LogTextBox(Client.GetDictText("LicenseBelow"));
            LogTextBox(Environment.NewLine);
            LogTextBox(@"Sightstone.Patcher, League of Legends Custom Patcher
    Copyright (C) 2015  eddy5641

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.");
            Load();
        }

        private void Load()
        {
            //Load server status
            Status();
            //Load Champions with this thread
            
            var x = new Thread(() =>
                {
                    using (var client = new WebClient())
                    {
                        var latestAir =
                            client.DownloadString(
                                "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_NA").Split(
                                new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                        var pkgManifest =
                            client.DownloadString(
                                string.Format(
                                    "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/{0}/packages/files/packagemanifest",
                                    latestAir)).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                        var freeToPlayChamps = client.DownloadString(
                            "http://cdn.leagueoflegends.com/patcher/data/regions/na/champData/freeToPlayChamps.json");
                        var champsAsJson = JsonConvert.DeserializeObject<Champions>(freeToPlayChamps);
                        foreach (var champs in champsAsJson.champions)
                        {
                            FreeWeekChampion champItem = null;
                            //There is probably a better way to do this, but this will work if there
                            //are currently no images installed on the user's computer (first time install)
                            Client.RunAsyncOnUIThread(() => champItem = new FreeWeekChampion());
                            var champDataJson =
                                client.DownloadString(
                                    string.Format("http://cdn.leagueoflegends.com/patcher/data/locales/en_US/champData/champData{0}.json", champs.id));
                            var champsDataAsJson = JsonConvert.DeserializeObject<Dictionary<String, object>>(champDataJson);
                            Client.RunAsyncOnUIThread(() => champItem.Tag = champsDataAsJson);
                            Client.RunAsyncOnUIThread(() => champItem.ChampName.Content = champsDataAsJson["key"]);
                            foreach (var stream in from data in pkgManifest where data.Contains((string)champsDataAsJson["key"] + "_0.") select (HttpWebRequest)WebRequest.Create("http://l3cdn.riotgames.com/releases/live" + data.Split(',')[0]) into httpWebRequest select (HttpWebResponse)httpWebRequest.GetResponse() into httpWebReponse select httpWebReponse.GetResponseStream())
                            {
                                MemoryStream stream1 = new MemoryStream();
                                stream.CopyTo(stream1);
                                Client.RunAsyncOnUIThread(() => champItem.Img.Source = ToWpfBitmap(Image.FromStream(stream1)));
                                stream.Close();
                                stream.Dispose();
                            }

                            Client.RunOnUIThread(() => champView.Items.Add(champItem));
                        }
                        Client.RunOnUIThread(() =>
                        {
                            Client.Win.Visibility = Visibility.Visible;
                            Client.Win.Show();
                            Client.SplashPage.Close();
                        });
                        if (Settings.Default.FirstStart)
                        {
                            Client.RunOnUIThread(() => Client.OverlayGrid.Visibility = Visibility.Visible);
                        }
                    }
                });
            x.Start();
            
            downloadWorker = new BackgroundWorker();

            downloadWorker.DoWork += Worker_DoWork;
            downloadWorker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Download();
        }

        private bool loaded;
        private BackgroundWorker downloadWorker;
        private List<DownloadFile> files;

        public StreamWriter logFile { get; private set; }

        /// <summary>
        /// Download the needed files to run Sightstone
        /// </summary>
        public void Download()
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.RegionName) || DownloadStarted)
                return;
            if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "PatchData")))
                Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "PatchData"));
            DownloadStarted = true;
            var region = MainRegion.GetMainRegion(Settings.Default.RegionName);
            files = new List<DownloadFile>();
            var downloader = new Downloader();

            //AddGameClientUpdateFiles(region);
            var manifest = LeagueDownloadLogic.AirGetUris(region);
            if (manifest.Count() == 0)
            {
                DownloadCompleted(false);
                return;
            }
            downloadTheme(manifest);
            foreach ( var clientFiles in manifest)
            {
                //Install sound files
                if (clientFiles.uri.ToString().Contains("sounds"))
                {
                    files.Add(new DownloadFile
                    {
                        DownloadUri = clientFiles.uri,
                        FileSize = clientFiles.size,
                        OutputPath = new[]
                        {
                            Path.Combine(Client.ExecutingDirectory,
                                clientFiles.uri.ToString()
                                .Split(new[] { "/files/" }, StringSplitOptions.None)[1]
                                .Replace("assets", "Assets"))},
                        OverrideFiles = true
                    });
                }
                //Download images of champs
                else if (clientFiles.uri.ToString().Contains("champions"))
                {
                    if (!clientFiles.uri.ToString().Contains("_Square_0"))
                    {
                        files.Add(new DownloadFile
                        {
                            DownloadUri = clientFiles.uri,
                            FileSize = clientFiles.size,
                            OutputPath =
                                    new[]
                                    {
                                        Path.Combine(Client.ExecutingDirectory,
                                            clientFiles.uri.ToString()
                                            .Split(new[] { "/files/" }, StringSplitOptions.None)[1]
                                            .Replace("assets", "Assets"))
                                    },
                            OverrideFiles = true
                        });
                    }
                    else
                    {
                        files.Add(new DownloadFile
                        {
                            DownloadUri = clientFiles.uri,
                            FileSize = clientFiles.size,
                            OutputPath =
                                    new[]
                                    {
                                        Path.Combine(Client.ExecutingDirectory,
                                            clientFiles.uri.ToString()
                                            .Split(new[] { "/files/" }, StringSplitOptions.None)[1]
                                            .Replace("assets", "Assets")).Replace("/images", ""),
                                        Path.Combine(Client.ExecutingDirectory, 
                                            clientFiles.uri.ToString()
                                            .Split(new[] { "/files/" },StringSplitOptions.None)[1].
                                            Replace("assets", "Assets"))
                                            .Replace("champions", "champion").Replace("/images", "")


                                    },
                            OverrideFiles = true
                        });
                    }
                    //If it is a square save it in the champion folder as well
                }
                else if (clientFiles.uri.ToString().Contains("images/runes"))
                {
                    files.Add(new DownloadFile
                    {
                        DownloadUri = clientFiles.uri,
                        FileSize = clientFiles.size,
                        OutputPath =
                                    new[]
                                    {
                                        Path.Combine(Client.ExecutingDirectory,
                                            clientFiles.uri.ToString()
                                            .Split(new[] { "/files/" }, StringSplitOptions.None)[1]
                                            .Replace(@"assets/images/runes", @"Assets/rune"))

                                    },
                        OverrideFiles = true
                    });
                }
                else if (clientFiles.uri.ToString().Contains("abilities"))
                {
                    if (clientFiles.uri.ToString().Contains("Passive"))
                    {
                        files.Add(new DownloadFile
                        {
                            DownloadUri = clientFiles.uri,
                            FileSize = clientFiles.size,
                            OutputPath =
                                    new[]
                                    {
                                        Path.Combine(Client.ExecutingDirectory,
                                            clientFiles.uri.ToString()
                                            .Split(new[] { "/files/" }, StringSplitOptions.None)[1]
                                            .Replace(@"assets/images/abilities", @"Assets/spell")),
                                        Path.Combine(Client.ExecutingDirectory,
                                            clientFiles.uri.ToString()
                                            .Split(new[] { "/files/" }, StringSplitOptions.None)[1]
                                            .Replace(@"assets/images/abilities", @"Assets/passive"))

                                    },
                            OverrideFiles = true
                        });
                    }
                    else
                    {
                        files.Add(new DownloadFile
                        {
                            DownloadUri = clientFiles.uri,
                            FileSize = clientFiles.size,
                            OutputPath =
                                    new[]
                                    {
                                        Path.Combine(Client.ExecutingDirectory,
                                        clientFiles.uri.ToString()
                                                            .Split(new[] { "/files/" }, StringSplitOptions.None)[1]
                                                            .Replace(@"assets/images/abilities", @"Assets/spell"))

                                    },
                            OverrideFiles = true
                        });
                    }
                }
                else if (clientFiles.uri.ToString().Contains("imagePacks"))
                {
                    if (SWFextract == null)
                        SWFextract = new List<updateSWF>();
                    files.Add(new DownloadFile
                    {
                        DownloadUri = clientFiles.uri,
                        FileSize = clientFiles.size,
                        OutputPath = new []
                        {
                            Path.Combine(Client.ExecutingDirectory, "Assets", "swf", clientFiles.uri.ToString()
                            .Split(new[] { "/imagePacks/" }, StringSplitOptions.None)[1])
                        },
                        OverrideFiles = true
                    });
                    var item = new updateSWF()
                    {
                        savePath = Path.Combine(Client.ExecutingDirectory, "Assets", "swf", Path.GetFileNameWithoutExtension(clientFiles.uri.ToString())),
                        swfPath = Path.Combine(Client.ExecutingDirectory, "Assets", "swf", Path.GetFileName(clientFiles.uri.ToString())),
                    };
                    SWFextract.Add(item);
                }

            }
            downloader.OnFinishedDownloading += DownloadCompleted;
            downloader.OnDownloadProgressChanged += DownloadChange;
            downloader.DownloadMultipleFiles(files);
            
        }

        private void AddGameClientUpdateFiles(MainRegion region)
        {
            var root = GetLolRootPath();
            var lolClientSlnVersion = LeagueDownloadLogic.GetLolClientSlnVersion(region)[0];
            var lolClientVersion = LeagueDownloadLogic.GetLolClientClientVersion(region)[0];
            switch (region.RegionType)
            {
                case RegionType.KR:
                case RegionType.Riot:
                    var uris = LeagueDownloadLogic.GetUris(region);
                    files.AddRange(uris.Select(toDl => new DownloadFile
                    {
                        DownloadUri = toDl.uri,
                        FileSize = toDl.size,
                        OutputPath = new[]
                            {
                                Path.Combine(root, "RADS", "solutions", "lol_game_client_sln", "releases",
                                lolClientSlnVersion, "deploy", toDl.uri.ToString().Split(new[] { "/files/" },
                                    StringSplitOptions.None)[1]),

                                Path.Combine(root, "RADS", "projects", "lol_game_client", "releases",
                                lolClientVersion, "deploy", toDl.uri.ToString().Split(new[] { "/files/" },
                                    StringSplitOptions.None)[1])
                            },
                        OverrideFiles = true
                    }).ToList());
                    break;
                case RegionType.PBE:
                    files.AddRange(LeagueDownloadLogic.GetUris(region).Select(toDl => new DownloadFile
                    {
                        DownloadUri = toDl.uri,
                        FileSize = toDl.size,
                        OutputPath = new[] {Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client", "releases",
                                LeagueDownloadLogic.GetLolClientClientVersion(region)[0], "deploy", toDl.uri.ToString().Split(new[] { "/files/" },
                                    StringSplitOptions.None)[1])},
                        OverrideFiles = true
                    }).ToList());
                    break;
                case RegionType.Garena:
                    throw new NotImplementedException("Garena update logic has to be observed");
            }
        }

        private void downloadTheme(UriAndSize[] manifest)
        {
            try
            {
                UriAndSize[] fileMetaData = manifest.Skip(1).ToArray();
                var updateRegion = Logic.UpdateRegion.BaseUpdateRegion.GetUpdateRegion("Live");

                if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "themes")))
                    Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "themes"));

                foreach (var uriAndSize in fileMetaData)
                {
                    var s = uriAndSize.uri.ToString();
                    if (string.IsNullOrEmpty(s))
                        continue;

                    string location = s.Split(',')[0];
                    string savePlace = location.Split(new[] { "/files/" }, StringSplitOptions.None)[1];
                    if (savePlace.Contains("theme.properties"))
                    {
                        using (var newClient = new WebClient())
                        {
                            LogTextBox("Checking Theme...");
                            newClient.DownloadFile(location,
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
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", "themedata")))
                    File.Delete(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", "themedata"));
                var themefile = File.Create(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", "themedata"));
                themefile.Write(Encoding.UTF8.GetBytes(theme), 0, Encoding.UTF8.GetBytes(theme).Count());
                themefile.Close();

                if (theme == "")
                    return;

                if (!Directory.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme)))
                    Directory.CreateDirectory(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme));
                else
                {
                    
                    return;
                }

                List<UriAndSize> themeLink = fileMetaData.Where(
                                   line => ((line.uri.AbsoluteUri.Contains("loop")) || 
                                        line.uri.AbsoluteUri.Contains("Loop")) &&
                                        line.uri.AbsoluteUri.Contains(theme)).ToList(); //loop is exacly the same as intro

                using (var newClient = new WebClient())
                {
                    foreach (var item in themeLink)
                    {
                        string fileName = item.uri.ToString().Split('/').Last();
                        if (File.Exists(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme, fileName)))
                            if (new FileInfo(Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme, fileName)).Length == item.size)
                                continue;
                        LogTextBox("Downloading " + fileName + " from http://l3cdn.riotgames.com");
                        newClient.DownloadFile(item.uri, Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme, fileName));

                    }
                }
                string[] flv = Directory.GetFiles(
                    Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme), "*.flv");

                foreach (var item in flv)
                {
                    var inputFile = new MediaFile
                    {
                        Filename =
                        Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme, item)
                    };
                    var outputFile = new MediaFile
                    {
                        Filename =
                        Path.Combine(Client.ExecutingDirectory, "Assets", "themes", theme, item).Replace(".flv", ".mp4")
                    };

                    using (var engine = new Engine())
                    {
                        engine.Convert(inputFile, outputFile);
                    }
                }
            }
            catch
            {
            }
        }

        private void DownloadChange(double downloaded, double toDownload)
        {
            Client.RunOnUIThread(() =>
                {
                    var percentage = downloaded / toDownload * 100;
                    TotalBar.Value = int.Parse(Math.Truncate(percentage).ToString(CultureInfo.CurrentCulture));
                    Client.Win.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    Client.Win.TaskbarItemInfo.ProgressValue = percentage / 100;
                });
        }

        private void DownloadCompleted(bool updated)
        {
            if (updated)
            {
                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "PatchData", "LC_LOLAir.Version")))
                {
                    File.Delete(Path.Combine(Client.ExecutingDirectory, "PatchData", "LC_LOLAir.Version"));
                }

                var latestAirs = LeagueDownloadLogic.GetLolClientVersion(Client.Region);

                File.WriteAllText(Path.Combine(Client.ExecutingDirectory, "PatchData", "LC_LOLAir.Version"), latestAirs[0]);

                if (SWFextract != null)
                {
                    foreach (var item in SWFextract)
                    {
                        SWFImagePacks.SWFextract(item.swfPath, item.savePath);
                    }
                }
                //TODO: Converters
            }
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
            {
                PlayButton.IsEnabled = true;
                FinishedGrid.Visibility = Visibility.Visible;
                Client.Win.TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            }));
        }

        private void Status()
        {
            var t = new Thread(() =>
                {
                    using (var client = new WebClient())
                    {
                        var news = client.DownloadString("http://ll.leagueoflegends.com/pages/launcher/na").
                            Replace("refreshContent(", "").Replace(");", "");
                        var newsJson = JsonConvert.DeserializeObject<Rootobject>(news);
                        Client.RunOnUIThread(() =>
                            {
                                var item = new CurrentStatus
                                {
                                    StatusLabel = { Content = Client.GetDictText("RiotServers") },
                                    Tag = new Uri("https://na.leagueoflegends.com")
                                };
                                item.UpdateStatus(
                                    newsJson.status ? PatcherElements.Status.Down : PatcherElements.Status.Up);
                                item.MouseDown += (o, e) => Changed(o);
                                StatusView.Items.Add(item);
                            });
                        var request = WebRequest.Create("http://legendaryclient.net");
                        using (var response = (HttpWebResponse)request.GetResponse())
                        {
                            var statusCode = response.StatusCode;
                            Client.RunOnUIThread(() =>
                            {
                                var item = new CurrentStatus
                                {
                                    StatusLabel = { Content = Client.GetDictText("SightstoneWebsite") },
                                    Tag = new Uri("http://legendaryclient.net")
                                };
                                item.UpdateStatus(
                                    statusCode != HttpStatusCode.OK
                                        ? PatcherElements.Status.Down
                                        : PatcherElements.Status.Up);
                                item.MouseDown += (o, e) => Changed(o);
                                StatusView.Items.Add(item);
                            });
                        }

                        /*
                        request = WebRequest.Create("http://forums.Sightstone.net");
                        response = (HttpWebResponse)request.GetResponse();
                        Client.RunOnUIThread(() =>
                        {
                            var item = new CurrentStatus
                            {
                                StatusLabel = { Content = "Sightstone forums" },
                                Tag = new Uri("http://forums.Sightstone.net")
                            };
                            item.UpdateStatus(
                                response.StatusCode != HttpStatusCode.OK
                                    ? PatcherElements.Status.Down
                                    : PatcherElements.Status.Up);
                            item.MouseDown += (o, e) => Changed(o);
                            StatusView.Items.Add(item);
                        });
                        //*/
                        if (loaded)
                            return;
                        //Might as well load news
                        foreach (var n in newsJson.news)
                        {
                            var n1 = n;
                            Client.RunOnUIThread(() =>
                                {
                                    var item = new NewsItem
                                    {
                                        TitleLabel = { Content = n1.title },
                                        TimeLabel = { Content = n1.date },
                                        ContentBox = { Visibility = Visibility.Hidden },
                                        Height = 26,
                                        Tag = new Uri(n1.url.Replace(@"\/", @"/")),
                                        Width = 300
                                    };
                                    item.MouseDown += (o, e) => Changed(o);
                                    NewsBox.Items.Add(item);
                                });
                        }
                        foreach (var x in newsJson.community)
                        {
                            var x1 = x;
                            Client.RunOnUIThread(() =>
                            {
                                var item = new NewsItem { TitleLabel = { Content = x1.title }, Width = 300 };
                                item.ContentBox.AppendText(x1.promoText);
                                item.TimeLabel.Visibility = Visibility.Hidden;
                                item.Tag = new Dictionary<string, object>
                                {
                                    { "imgUrl", x1.imageUrl.Replace(@"\/", @"/") },
                                    { "thumbUrl", x1.thumbUrl.Replace(@"\/", @"/") },
                                    { "linkUrl", x1.linkUrl.Replace(@"\/", @"/") }
                                };
                                item.MouseDown += (o,e) => Changed(o);
                                NewsBox.Items.Add(item);
                            });
                        }
                        loaded = true;
                    }
                });
            t.Start();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Changed(sender);
        }

        private void Changed(object sender)
        {
            if (sender.GetType() == typeof(NewsItem))
            {
                if (!(((NewsItem)sender).Tag is string))
                    return;
                WebBrower.Source = (Uri)(((NewsItem)sender).Tag);
            }
            else if (sender.GetType() == typeof(CurrentStatus))
            {
                var x = (((CurrentStatus) sender).Tag);
                if (x.GetType() != typeof(Directory))
                    WebBrower.Source = (Uri)x;
                else WebBrower.Source = new Uri((string)((Dictionary<string, object>)x)["linkUrl"]);
            }
        }

        private void Update_OnClick(object sender, RoutedEventArgs e)
        {
            StatusView.Items.Clear();
            Status();
        }

        public static BitmapSource ToWpfBitmap(Image bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                var result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();

                return result;
            }
        }

        /// <summary>
        ///     Starts Sightstone
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayClick(object sender, RoutedEventArgs e)
        {
            Process.Start("Sightstone.exe");
            Environment.Exit(0);
        }

        /// <summary>
        ///     A Simple Logger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomainFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            //Disregard PVPNetSpam
            if (e.Exception.Message.Contains("too small for an int") ||
                e.Exception.Message.Contains("Constructor on type "))
                return;
            Log("A first chance exception was thrown", "EXCEPTION");
            Log(e.Exception.Message, "EXCEPTION");
            Log(e.Exception.StackTrace, "EXCEPTION");
        }

        /// <summary>
        ///     A simple Log Writer
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="type"></param>
        public void Log(string lines, string type = "LOG")
        {
            if (logFile == null)
                logFile = new StreamWriter(Path.Combine(_executingDirectory, "SightstonePatcher.log"), true);
            logFile.WriteLine("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(),
                type, lines);
            logFile.Close();
        }

        public void LogTextBox(string s)
        {
            Dispatcher.BeginInvoke(new Action(() => Logbox.Text += s + Environment.NewLine));
        }

        private void SwitchClick(object sender, RoutedEventArgs e)
        {
            if (NewsGrid.Visibility == Visibility.Visible)
            {
                LogGrid.Visibility = Visibility.Visible;
                NewsGrid.Visibility = Visibility.Hidden;
                SwitchButton.Content = Client.GetDictText("ShowNews");
            }
            else
            {
                SwitchButton.Content = Client.GetDictText("ShowLogBox");
                LogGrid.Visibility = Visibility.Hidden;
                NewsGrid.Visibility = Visibility.Visible;
            }
        }
        private string GetLolRootPath()
        {
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
                    var regKey = Registry.CurrentUser.CreateSubKey("Sightstone.Patcher");
                    if (regKey == null) return value.ToString();
                    regKey.SetValue(
                        value.ToString().Contains("lol.exe") ? "GarenaLocation" : "LoLLocation",
                        value.ToString());
                    regKey.Close();
                    return value.ToString();
                }
                catch
                {
                    //Ignored
                }
            }

            if (MessageBox.Show("Do you have lol installed?", "League not found", MessageBoxButton.YesNo) ==
                MessageBoxResult.No)
            {
                Registry.SetValue(@"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\SightstoneLol", 
                    "Path", Path.Combine(Client.ExecutingDirectory, "LeagueClient"));
                return Path.Combine(Client.ExecutingDirectory, "LeagueClient");
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
            
            return findLeagueDialog.FileName.Replace("lol.launcher.exe", string.Empty).Replace("lol.launcher.admin.exe", string.Empty);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Champion
    {
        public int id { get; set; }
        public bool active { get; set; }
        public bool botEnabled { get; set; }
        public bool freeToPlay { get; set; }
        public bool botMmEnabled { get; set; }
        public bool rankedPlayEnabled { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Champions
    {
        public List<Champion> champions { get; set; }
    }

    //News
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class News
    {
        public string title { get; set; }
        public string url { get; set; }
        public string date { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Community
    {
        public string title { get; set; }
        public string promoText { get; set; }
        public string imageUrl { get; set; }
        public string thumbUrl { get; set; }
        public string linkUrl { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class Rootobject
    {
        public bool status { get; set; }
        public int serverStatus { get; set; }
        public List<News> news { get; set; }
        public List<Community> community { get; set; }
    }

    public class updateSWF
    {
        public string savePath { get; set; }
        public string swfPath { get; set; }
    }

   
}