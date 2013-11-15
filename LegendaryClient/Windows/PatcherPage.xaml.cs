using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using LegendaryClient.Logic;

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
                string CurrentMD5 = GetMd5();
                string CurrentKyokuMD5 = GetMd5(true);
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
                    /*if (versionArray[0] != CurrentMD5)
                    {
                        this.Invoke(new MethodInvoker(delegate
                        {
                            CheckingForUpdatesLabel.Text = "Downloading latest Client of Legends...";
                        }));
                        using (WebClient client = new WebClient())
                        {
                            client.DownloadFile(versionArray[2], "COL.ZIP");
                        }
                        Directory.CreateDirectory("Patch");
                        System.IO.Compression.ZipFile.ExtractToDirectory("COL.ZIP", "Patch");                    
                        File.Delete("COL.ZIP");
                        System.Diagnostics.Process.Start("Patcher.exe");
                        Environment.Exit(0);
                    }*/

                    /*if (versionArray[1] != CurrentKyokuMD5)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            CurrentStatusLabel.Content = "Downloading latest Kyoku...";
                        }));
                        client.DownloadFile(versionArray[3], "KYOKU.ZIP");
                        Directory.CreateDirectory("Patch");
                        ZipFile.ExtractToDirectory("KYOKU.ZIP", "Patch");
                        File.Delete("KYOKU.ZIP");
                        if (Directory.Exists("Patch"))
                        {
                            foreach (string newPath in Directory.GetFiles("Patch", "*.*", SearchOption.AllDirectories))
                                File.Copy(newPath, newPath.Replace("Patch", "."), true);
                        }
                        System.IO.DirectoryInfo PatchInfo = new DirectoryInfo("Patch");

                        foreach (FileInfo file in PatchInfo.GetFiles())
                        {
                            file.Delete();
                        }
                        foreach (DirectoryInfo dir in PatchInfo.GetDirectories())
                        {
                            dir.Delete(true);
                        }
                        Directory.Delete("Patch");
                    }*/

                    var KyokuProcess = new System.Diagnostics.Process();
                    KyokuProcess.StartInfo.UseShellExecute = false;
                    /*KyokuProcess.StartInfo.RedirectStandardOutput = true;
                    KyokuProcess.StartInfo.RedirectStandardError = true;
                    KyokuProcess.EnableRaisingEvents = true;
                    KyokuProcess.StartInfo.CreateNoWindow = true;
                    KyokuProcess.OutputDataReceived += p_OutputDataReceived;
                    KyokuProcess.ErrorDataReceived += p_ErrorDataReceived;*/
                    KyokuProcess.StartInfo.FileName = Path.Combine(Directory.GetCurrentDirectory(), "Kyoku.exe");
                    //KyokuProcess.Start();
                    /*KyokuProcess.BeginOutputReadLine();
                    KyokuProcess.BeginErrorReadLine();*/

                    /*while (!KyokuProcess.WaitForExit(1000)) { }
                    if (KyokuProcess.ExitCode != 991)
                    {
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            CurrentStatusLabel.Content = "League of Legends was unable to patch. You cannot play at this time.";
                            CurrentProgressBar.Value = 0;
                        }));
                        return;
                    }
                    KyokuProcess = null;*/
                }

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

        private string GetMd5(bool IsKyoku = false)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            FileInfo fi = null;
            FileStream stream = null;
            if (!IsKyoku)
            {
                fi = new FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                stream = File.Open(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName, FileMode.Open, FileAccess.Read);
            }
            else
            {
                if (File.Exists("Kyoku.exe"))
                {
                    fi = new FileInfo("Kyoku.exe");
                    stream = File.Open("Kyoku.exe", FileMode.Open, FileAccess.Read);
                }
                else
                {
                    return "";
                }
            }

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
