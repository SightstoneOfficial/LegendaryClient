#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using LegendaryClient.Patcher.Logic;
using LegendaryClient.Patcher.PatcherElements;
using Newtonsoft.Json;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Image = System.Drawing.Image;
using LegendaryClient.Patcher.Properties;

#endregion

namespace LegendaryClient.Patcher.Pages
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

        public PatcherPage()
        {
            InitializeComponent();
            _executingDirectory = Client.ExecutingDirectory;
            _isLogVisible = false;

            _executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            if (_executingDirectory != null &&
                File.Exists(Path.Combine(_executingDirectory, "LegendaryClientPatcher.log")))
            {
                File.Delete(Path.Combine(_executingDirectory, "LegendaryClientPatcher.log"));
            }
            LogTextBox("You must have The following license below to use LegendaryClient.Patcher");
            LogTextBox(Environment.NewLine);
            LogTextBox(@"Legendaryclient.Patcher, League of Legendary Custom Patcher
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
                            Client.splashPage.Close();
                        });
                        if (Settings.Default.FirstStart)
                        {
                            Client.RunOnUIThread(() => Client.OverlayGrid.Visibility = Visibility.Visible);
                        }
                    }
                });
            x.Start();
        }

        private bool loaded;

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
                                    StatusLabel = { Content = "LOLServers" },
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
                                StatusLabel = { Content = "LegendaryClient website" },
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
                        request = WebRequest.Create("http://forums.legendaryclient.net");
                        response = (HttpWebResponse)request.GetResponse();
                        Client.RunOnUIThread(() =>
                        {
                            var item = new CurrentStatus
                            {
                                StatusLabel = { Content = "LegendaryClient forums" },
                                Tag = new Uri("http://forums.legendaryclient.net")
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
                            Client.RunOnUIThread(() =>
                                {
                                    var item = new NewsItem
                                    {
                                        TitleLabel = { Content = n.title },
                                        TimeLabel = { Content = n.date },
                                        ContentBox = { Visibility = Visibility.Hidden },
                                        Height = 26,
                                        Tag = new Uri(n.url.Replace(@"\/", @"/")),
                                        Width = 300
                                    };
                                    item.MouseDown += (o, e) => Changed(o);
                                    NewsBox.Items.Add(item);
                                });
                        }
                        foreach (var x in newsJson.community)
                        {
                            Client.RunOnUIThread(() =>
                            {
                                var item = new NewsItem { TitleLabel = { Content = x.title }, Width = 300 };
                                item.ContentBox.AppendText(x.promoText);
                                item.TimeLabel.Visibility = Visibility.Hidden;
                                item.Tag = new Dictionary<string, object>
                                {
                                    { "imgUrl", x.imageUrl.Replace(@"\/", @"/") },
                                    { "thumbUrl", x.thumbUrl.Replace(@"\/", @"/") },
                                    { "linkUrl", x.linkUrl.Replace(@"\/", @"/") }
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
        ///     Swiches the command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            if (_isLogVisible)
            {
                _isLogVisible = false;
                NewsGrid.Visibility = Visibility.Visible;
                LogGrid.Visibility = Visibility.Hidden;
            }
            else if (_isLogVisible == false)
            {
                _isLogVisible = true;
                NewsGrid.Visibility = Visibility.Hidden;
                LogGrid.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        ///     Starts LegendaryClient
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Play_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        ///     Highlights UnderButtons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverButtonLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            var bc = new BrushConverter();
            var brush = (Brush) bc.ConvertFrom("#41B1E1");
            UnderButtonLeft.Foreground = brush;
            UnderButtonLeft.Background = brush;
        }

        /// <summary>
        ///     Highlightes UnderButtons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OverButtonRight_MouseEnter(object sender, MouseEventArgs e)
        {
            var bc = new BrushConverter();
            var brush = (Brush) bc.ConvertFrom("#41B1E1");
            UnderButtonRight.Foreground = brush;
            UnderButtonRight.Background = brush;
        }

        /// <summary>
        ///     Makes the UnderButtons Return to normal
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReturnButtonsToNumbers_MouseLeage(object sender, MouseEventArgs e)
        {
            var bc = new BrushConverter();
            var brush = (Brush) bc.ConvertFrom("Transparent");
            UnderButtonRight.Foreground = brush;
            UnderButtonLeft.Foreground = brush;
            UnderButtonRight.Background = brush;
            UnderButtonLeft.Background = brush;
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
            var file = new StreamWriter(Path.Combine(_executingDirectory, "LegendaryClientPatcher.log"), true);
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
                OverButtonLeft.Content = "Show News";
            }
            else
            {
                OverButtonLeft.Content = "Show Log TextBox";
                LogGrid.Visibility = Visibility.Hidden;
                NewsGrid.Visibility = Visibility.Visible;
            }
        }
    }
    public class Champion
    {
        public int id { get; set; }
        public bool active { get; set; }
        public bool botEnabled { get; set; }
        public bool freeToPlay { get; set; }
        public bool botMmEnabled { get; set; }
        public bool rankedPlayEnabled { get; set; }
    }

    public class Champions
    {
        public List<Champion> champions { get; set; }
    }

    //News
    public class News
    {
        public string title { get; set; }
        public string url { get; set; }
        public string date { get; set; }
    }

    public class Community
    {
        public string title { get; set; }
        public string promoText { get; set; }
        public string imageUrl { get; set; }
        public string thumbUrl { get; set; }
        public string linkUrl { get; set; }
    }

    public class Rootobject
    {
        public bool status { get; set; }
        public int serverStatus { get; set; }
        public List<News> news { get; set; }
        public List<Community> community { get; set; }
    }
}