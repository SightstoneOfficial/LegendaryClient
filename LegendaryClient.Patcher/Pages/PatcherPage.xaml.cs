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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Brush = System.Windows.Media.Brush;
using Image = System.Drawing.Image;

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
        private readonly String _executingDirectory;
        private Boolean _isLogVisible;

        public PatcherPage()
        {
            InitializeComponent();
            _executingDirectory = Client.ExecutingDirectory;
            _isLogVisible = false;
            //Finds where the patcher was started
            if (!File.Exists(Path.Combine(_executingDirectory, "Patcher.settings")))
            {
                FileStream x = File.Create(Path.Combine(_executingDirectory, "Patcher.settings"));
                //Client.OverlayGrid.Content
                x.Close();
            }
            else
                File.ReadAllText(Path.Combine(_executingDirectory, "Patcher.settings"));


            _executingDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            if (_executingDirectory != null &&
                File.Exists(Path.Combine(_executingDirectory, "LegendaryClientPatcher.log")))
            {
                File.Delete(Path.Combine(_executingDirectory, "LegendaryClientPatcher.log"));
            }


            Load();
            //LogTextBox(CreateConfigurationmanifest());
        }

        private void Load()
        {
            using (var client = new WebClient())
            {
                var freeToPlayChamps = client.DownloadString(
                    "http://cdn.leagueoflegends.com/patcher/data/regions/na/champData/freeToPlayChamps.json");
                var champsAsJson = JsonConvert.DeserializeObject<Champions>(freeToPlayChamps);
                foreach (var champs in champsAsJson.champions)
                {
                    var champItem = new FreeWeekChampion();
                    var champDataJson =
                        client.DownloadString(
                            string.Format("http://cdn.leagueoflegends.com/patcher/data/locales/en_US/champData/champData{0}.json", champs.id));
                    var champsDataAsJson = JsonConvert.DeserializeObject<Dictionary<String, Object>>(champDataJson);
                    champItem.ChampName.Content = champsDataAsJson["key"];
                    var latestAir =
                        client.DownloadString(
                            "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_NA").Split(
                            new[] {Environment.NewLine}, StringSplitOptions.None)[0];
                    var pkgManifest =
                        client.DownloadString(
                            string.Format(
                                "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/{0}/packages/files/packagemanifest",
                                latestAir)).Split(new []{Environment.NewLine}, StringSplitOptions.None);
                    foreach (var stream in from data in pkgManifest where data.Contains("square") && data.Contains((string) champsDataAsJson["name"]) select (HttpWebRequest)HttpWebRequest.Create("http://l3cdn.riotgames.com/releases/live" + data.Split(',')[0]) into httpWebRequest select (HttpWebResponse)httpWebRequest.GetResponse() into httpWebReponse select httpWebReponse.GetResponseStream()) {
                        champItem.Img.Source = ToWpfBitmap(Image.FromStream(stream));
                    }
                    champView.Items.Add(champItem);
                }
            }
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
            if (e.Exception.Message.Contains("too small for an Int32") ||
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
        public void Log(String lines, String type = "LOG")
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
}