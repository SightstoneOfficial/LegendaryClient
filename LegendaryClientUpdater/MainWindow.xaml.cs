using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using MahApps.Metro.Controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;

namespace LegendaryClientUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        Int64 zipSize;
        Int64 downloadedAmount;
        int extractedAmount;

        public MainWindow()
        {
            InitializeComponent();
            downloadedAmount = 0;
            extractedAmount = 0;
        }

        private void Form_Loaded(object sender, RoutedEventArgs e)
        {
            using (WebClient client = new WebClient())
            {
                client.OpenRead("http://dispersia.github.io/NewLegendaryClient.zip");
                zipSize = Convert.ToInt64(client.ResponseHeaders["Content-Length"]);

                client.DownloadProgressChanged += (s, es) =>
                {
                    DownloadProgressBar.Value = es.ProgressPercentage;
                    downloadedAmount = es.BytesReceived;
                    DownloadLabel.Content = "Downloading Zip: [" + downloadedAmount + "/" + zipSize + "]";
                };

                client.DownloadFileCompleted += (s, es) =>
                {
                    ZipFile zf = null;
                    try
                    {
                        FileStream fs = File.OpenRead("NewLegendaryClient.zip");
                        zf = new ZipFile(fs);
                        DownloadLabel.Content = "Extract Files [" + extractedAmount + "/" + zf.Count + "]";
                        DownloadProgressBar.Value = 0;
                        DownloadProgressBar.Maximum = zf.Count;
                        foreach (ZipEntry zipEntry in zf)
                        {
                            if (!zipEntry.IsFile)
                            {
                                continue;
                            }
                            string fileName = zipEntry.Name;
                            byte[] buffer = new byte[4096];
                            Stream zipStream = zf.GetInputStream(zipEntry);

                            using (FileStream streamWriter = File.Create(zipEntry.Name))
                            {
                                StreamUtils.Copy(zipStream, streamWriter, buffer);
                            }
                            DownloadLabel.Content = "Extracting File [" + ++extractedAmount + "/" + zf.Count + "]";
                            DownloadProgressBar.Value++;
                            if (extractedAmount == zf.Count)
                            {
                                Process.Start("LegendaryClient.exe");
                                Environment.Exit(Environment.ExitCode);
                            }
                        }
                    }
                    finally
                    {
                        if (zf != null)
                        {
                            zf.IsStreamOwner = true;
                            zf.Close();
                        }
                        if (File.Exists("LegendaryClient.zip"))
                            File.Delete("LegendaryClient.zip");
                    }
                };

                client.DownloadFileAsync(new Uri("http://dispersia.github.io/NewLegendaryClient.zip"), "NewLegendaryClient.zip");
            }
        }
    }
}
