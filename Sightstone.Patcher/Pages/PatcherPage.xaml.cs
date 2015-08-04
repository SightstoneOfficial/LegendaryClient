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
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Sightstone.Patcher.Logic.Region;
using Brush = System.Windows.Media.Brush;
using Image = System.Drawing.Image;
using Sightstone.Patcher.Properties;

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
        private bool _isLogVisible;
        public bool downloadStarted;

        public PatcherPage()
        {
            InitializeComponent();
            _executingDirectory = Client.ExecutingDirectory;
            _isLogVisible = false;

            _executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            if (_executingDirectory != null &&
                File.Exists(Path.Combine(_executingDirectory, "SightstonePatcher.log")))
            {
                File.Delete(Path.Combine(_executingDirectory, "SightstonePatcher.log"));
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
                            var latestAir =
                                client.DownloadString(
                                    "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_NA").Split(
                                    new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                            var pkgManifest =
                                client.DownloadString(
                                    string.Format(
                                        "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/{0}/packages/files/packagemanifest",
                                        latestAir)).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                            foreach (var stream in from data in pkgManifest where data.Contains((string)champsDataAsJson["key"] + "_0.") select (HttpWebRequest)WebRequest.Create("http://l3cdn.riotgames.com/releases/live" + data.Split(',')[0]) into httpWebRequest select (HttpWebResponse)httpWebRequest.GetResponse() into httpWebReponse select httpWebReponse.GetResponseStream())
                            {
                                var stream1 = stream;
                                Client.RunAsyncOnUIThread(() => champItem.Img.Source = ToWpfBitmap(Image.FromStream(stream1)));
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

            Download();
        }

        private bool loaded;

        public void Download()
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.RegionName) || downloadStarted)
                return;
            downloadStarted = true;
            var region = MainRegion.GetMainRegion(Settings.Default.RegionName);
            var files = new List<DownloadFile>();
            var downloader = new Downloader();
            switch (region.RegionType)
            {
                //TODO: data, item, map, mastery

                case RegionType.KR:
                case RegionType.Riot:
                    {
                        files.AddRange(LeagueDownloadLogic.GetUris(region).Select(toDl => new DownloadFile
                        {
                            DownloadUri = toDl,
                            OutputPath = new [] {Path.Combine(Client.ExecutingDirectory, "RADS", "projects", "lol_game_client", "releases",
                                LeagueDownloadLogic.GetLolClientSlnVersion(region)[0], "deploy", toDl.ToString().Split(new[] { "/files" },
                                    StringSplitOptions.None)[1])} ,
                            OverrideFiles = true
                        }).ToList());
                        foreach (var clientFiles in LeagueDownloadLogic.ClientGetUris(region))
                        {
                            //Install sound files
                            if (clientFiles.ToString().Contains("sounds"))
                            {
                                files.Add(new DownloadFile
                                {
                                    DownloadUri = clientFiles,
                                    OutputPath = new []{Path.Combine(Client.ExecutingDirectory,
                                            clientFiles.ToString().Split(new[] { "/files" },
                                            StringSplitOptions.None)[1].Replace("assets", "Assets"))} ,
                                    OverrideFiles = true
                                });
                            }
                            //Download images of champs
                            else if (clientFiles.ToString().Contains("champions"))
                            {
                                if (!clientFiles.ToString().Contains("_Square_0"))
                                {
                                    files.Add(new DownloadFile
                                        {
                                            DownloadUri = clientFiles,
                                            OutputPath =
                                                new[]
                                                {
                                                    Path.Combine(
                                                        Client.ExecutingDirectory,
                                                        clientFiles.ToString()
                                                            .Split(new[] { "/files" }, StringSplitOptions.None)[1]
                                                            .Replace("assets", "Assets"))
                                                },
                                            OverrideFiles = true
                                        });
                                }
                                else
                                {
                                    files.Add(new DownloadFile
                                        {
                                            DownloadUri = clientFiles,
                                            OutputPath =
                                                new[]
                                                {
                                                    Path.Combine(
                                                        Client.ExecutingDirectory,
                                                        clientFiles.ToString()
                                                            .Split(new[] { "/files" }, StringSplitOptions.None)[1]
                                                            .Replace("assets", "Assets")).Replace("/images", ""),
                                                    Path.Combine(
                                                        Client.ExecutingDirectory,
                                                        clientFiles.ToString().Split(new[] { "/files" }, 
                                                        StringSplitOptions.None)[1].Replace("assets", "Assets")).
                                                        Replace("champions", "champion").Replace("/images", "")

                                                },
                                            OverrideFiles = true
                                        });
                                }
                                //If it is a square save it in the champion folder as well
                            }
                            else if (clientFiles.ToString().Contains("images/runes"))
                            {
                                files.Add(new DownloadFile
                                {
                                    DownloadUri = clientFiles,
                                    OutputPath =
                                                new[]
                                                {
                                                    Path.Combine(
                                                        Client.ExecutingDirectory,
                                                        clientFiles.ToString()
                                                            .Split(new[] { "/files" }, StringSplitOptions.None)[1]
                                                            .Replace(@"assets/images/runes", @"Assets/rune"))

                                                },
                                    OverrideFiles = true
                                });
                            }
                            else if (clientFiles.ToString().Contains("abilities"))
                            {

                                if (clientFiles.ToString().Contains("Passive"))
                                {
                                    files.Add(new DownloadFile
                                    {
                                        DownloadUri = clientFiles,
                                        OutputPath =
                                                new[]
                                                {
                                                    Path.Combine(
                                                        Client.ExecutingDirectory,
                                                        clientFiles.ToString()
                                                            .Split(new[] { "/files" }, StringSplitOptions.None)[1]
                                                            .Replace(@"assets/images/abilities", @"Assets/spell")),
                                                    Path.Combine(
                                                        Client.ExecutingDirectory,
                                                        clientFiles.ToString()
                                                            .Split(new[] { "/files" }, StringSplitOptions.None)[1]
                                                            .Replace(@"assets/images/abilities", @"Assets/passive"))

                                                },
                                        OverrideFiles = true
                                    });
                                }
                                else
                                {
                                    files.Add(new DownloadFile
                                    {
                                        DownloadUri = clientFiles,
                                        OutputPath =
                                                new[]
                                                {
                                                    Path.Combine(
                                                        Client.ExecutingDirectory,
                                                        clientFiles.ToString()
                                                            .Split(new[] { "/files" }, StringSplitOptions.None)[1]
                                                            .Replace(@"assets/images/abilities", @"Assets/spell"))

                                                },
                                        OverrideFiles = true
                                    });
                                }
                            }

                        }

                    }
                    break;
                case RegionType.PBE:
                    {
                        files.AddRange(LeagueDownloadLogic.GetUris(region).Select(toDl => new DownloadFile
                        {
                            DownloadUri = toDl,
                            OutputPath = new[] 
                            {
                                Path.Combine(Client.ExecutingDirectory, "PBE", "RADS", "projects", "lol_game_client", "releases",
                                LeagueDownloadLogic.GetLolClientSlnVersion(region)[0], "deploy", toDl.ToString().Split(new[] { "/files" },
                                    StringSplitOptions.None)[1])
                            } ,
                            OverrideFiles = true
                        }).ToList());
                    }
                    break;
            }

            downloader.DownloadMultipleFiles(files, TotalBar, DownloadCompleted);
        }

        private void DownloadCompleted()
        {
            FinishedGrid.Visibility = Visibility.Visible;
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
                        var response = (HttpWebResponse)request.GetResponse();
                        Client.RunOnUIThread(() =>
                        {
                            var item = new CurrentStatus
                            {
                                StatusLabel = { Content = Client.GetDictText("SightstoneWebsite") },
                                Tag = new Uri("http://legendaryclient.net")
                            };
                            item.UpdateStatus(
                                response.StatusCode != HttpStatusCode.OK
                                    ? PatcherElements.Status.Down
                                    : PatcherElements.Status.Up);
                            item.MouseDown += (o, e) => Changed(o);
                            StatusView.Items.Add(item);
                        });
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
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        ///     A Simple Logger
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
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
            var file = new StreamWriter(Path.Combine(_executingDirectory, "SightstonePatcher.log"), true);
            file.WriteLine("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(),
                type, lines);
            file.Close();
        }

        public void LogTextBox(string s)
        {
            Logbox.Text += s + Environment.NewLine;
        }

        private void OverButtonLeft_OnClick(object sender, RoutedEventArgs e)
        {
            if (NewsGrid.Visibility == Visibility.Visible)
            {
                LogGrid.Visibility = Visibility.Visible;
                NewsGrid.Visibility = Visibility.Hidden;
                OverButtonLeft.Content = Client.GetDictText("ShowNews");
            }
            else
            {
                OverButtonLeft.Content = Client.GetDictText("ShowLogBox");
                LogGrid.Visibility = Visibility.Hidden;
                NewsGrid.Visibility = Visibility.Visible;
            }
        }
        private string GetLolRootPath()
        {
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
}