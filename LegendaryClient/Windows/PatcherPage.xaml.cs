using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Awesomium.Core;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Patcher;

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
        }

        private void SkipPatchButton_Click(object sender, RoutedEventArgs e)
        {
            FinishPatching();
        }

        private void StartPatcher()
        {
            Thread bgThead = new Thread(() =>
            {
                /*string CurrentMD5 = GetMd5();
                string VersionString = "";
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
                try
                {
                    VersionString = client.DownloadString(new Uri("http://snowl.github.io/ClientOfLegends2/update.html"));
                }
                catch
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Could not retrieve update files!";
                    }));
                    return;
                }

                if (VersionString.Split('|').Length == 5)
                {
                    string[] versionArray = VersionString.Split('|');
                    if (versionArray[0] != CurrentMD5)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            CurrentStatusLabel.Content = "Downloading latest LegendaryClient...";
                        }));

                        client.DownloadFile(versionArray[2], "COL.ZIP");
                        Directory.CreateDirectory("Patch");
                        System.IO.Compression.ZipFile.ExtractToDirectory("COL.ZIP", "Patch");                    
                        File.Delete("COL.ZIP");
                        System.Diagnostics.Process.Start("Patcher.exe");
                        Environment.Exit(0);
                    }

                    RiotPatcher patcher = new RiotPatcher();
                    string s = patcher.GetDragon();
                    //client.DownloadFileAsync(s.ToUri(), "test.tgz");
                }*/

                FinishPatching();
            });

            bgThead.Start();
        }

        private void FinishPatching()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                Client.SwitchPage(new LoginPage());
            }));
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

    }
}
