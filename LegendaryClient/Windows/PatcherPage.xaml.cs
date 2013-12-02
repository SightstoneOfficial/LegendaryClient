using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using LegendaryClient.Logic;
using LegendaryClient.Logic.Patcher;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

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
                LogTextBox("Starting Patcher");

#if !Nopatch
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

                #region LegendaryClient
                string CurrentMD5 = GetMd5();
                LogTextBox("MD5: " + CurrentMD5);
                string VersionString = "";
                try
                {
                    VersionString = client.DownloadString(new Uri("http://legendaryclient.com/update.html"));
                }
                catch
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Could not retrieve update files!";
                    }));
                    return;
                }

                string[] VersionSplit = VersionString.Split('|');

                LogTextBox("Update data: " + VersionSplit[0] + "|" + VersionSplit[1]);

#if !DEBUG //Dont patch client while in DEBUG
                if (VersionSplit.Length == 3)
                {
                    string[] versionArray = VersionString.Split('|');
                    if (VersionSplit[0] != CurrentMD5)
                    {
                        LogTextBox("LegendaryClient needs to be updated");
                        /*Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            CurrentStatusLabel.Content = "Downloading latest LegendaryClient...";
                        }));

                        client.DownloadFile(versionArray[2], "COL.ZIP");
                        Directory.CreateDirectory("Patch");
                        System.IO.Compression.ZipFile.ExtractToDirectory("COL.ZIP", "Patch");
                        File.Delete("COL.ZIP");
                        System.Diagnostics.Process.Start("Patcher.exe");
                        Environment.Exit(0);*/
                    }
                }
#endif
                LogTextBox("LegendaryClient is up to date");
                #endregion

                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    TotalProgressLabel.Content = "20%";
                    TotalProgessBar.Value = 20;
                }));

                #region DDragon
                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                if (!Directory.Exists("Assets"))
                {
                    Directory.CreateDirectory("Assets");
                }
                if (!File.Exists(Path.Combine("Assets", "VERSION_DDRagon")))
                {
                    var VersionLOL = File.Create(Path.Combine("Assets", "VERSION_DDRagon"));
                    VersionLOL.Write(encoding.GetBytes("0.0.0"), 0, encoding.GetBytes("0.0.0").Length);
                    VersionLOL.Close();
                }

                RiotPatcher patcher = new RiotPatcher();
                string DDragonDownloadURL = patcher.GetDragon();
                LogTextBox("DataDragon Version: " + patcher.DDragonVersion);
                string DDragonVersion = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_DDragon"));
                LogTextBox("Current DataDragon Version: " + DDragonVersion);

                if (patcher.DDragonVersion != DDragonVersion)
                {
                    if (!Directory.Exists(Path.Combine("Assets", "temp")))
                    {
                        Directory.CreateDirectory(Path.Combine("Assets", "temp"));
                    }

                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Downloading DataDragon";
                    }));

                    client.DownloadFile(DDragonDownloadURL, Path.Combine("Assets", "dragontail-" + patcher.DDragonVersion + ".tgz"));

                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Extracting DataDragon";
                    }));

                    Stream inStream = File.OpenRead(Path.Combine("Assets", "dragontail-" + patcher.DDragonVersion + ".tgz"));
                    using (GZipInputStream gzipStream = new GZipInputStream(inStream))
                    {
                        TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                        tarArchive.ExtractContents(Path.Combine("Assets", "temp"));
                        tarArchive.CloseArchive();
                    }
                    inStream.Close();

                    Copy(Path.Combine("Assets", "temp", patcher.DDragonVersion, "data"), Path.Combine("Assets", "data"));
                    Copy(Path.Combine("Assets", "temp", patcher.DDragonVersion, "img"), Path.Combine("Assets"));
                    Directory.Delete(Path.Combine("Assets", "temp"), true);

                    var VersionDDragon = File.Create(Path.Combine("Assets", "VERSION_DDRagon"));
                    VersionDDragon.Write(encoding.GetBytes(patcher.DDragonVersion), 0, encoding.GetBytes(patcher.DDragonVersion).Length);
                    VersionDDragon.Close();
                }
                #endregion

                Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    TotalProgressLabel.Content = "40%";
                    TotalProgessBar.Value = 40;
                }));

                #region lol_air_client
                if (!File.Exists(Path.Combine("Assets", "VERSION_AIR")))
                {
                    var VersionAIR = File.Create(Path.Combine("Assets", "VERSION_AIR"));
                    VersionAIR.Write(encoding.GetBytes("0.0.0.0"), 0, encoding.GetBytes("0.0.0.0").Length);
                    VersionAIR.Close();
                }

                string LatestAIR = patcher.GetLatestAir();
                LogTextBox("Air Assets Version: " + LatestAIR);
                string AirVersion = File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "Assets", "VERSION_AIR"));
                LogTextBox("Current Air Assets Version: " + AirVersion);
                bool RetrieveCurrentInstallation = false;
                string AirLocation = "";

                if (AirVersion == "0.0.0.0")
                {
                    LogTextBox("Checking for existing League of Legends Installation");
                    AirLocation = Path.Combine("League of Legends", "RADS", "projects", "lol_air_client", "releases");
                    if (Directory.Exists(AirLocation))
                    {
                        RetrieveCurrentInstallation = true;
                    }
                    else if (Directory.Exists(Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", AirLocation)))
                    {
                        RetrieveCurrentInstallation = true;
                        AirLocation = Path.Combine(System.IO.Path.GetPathRoot(Environment.SystemDirectory), "Riot Games", AirLocation);
                    }
                    else
                    {
                        LogTextBox("Unable to find existing League of Legends. Copy your League of Legends folder into + "
                            + Client.ExecutingDirectory
                            + " to make the patching process quicker");
                    }

                    if (RetrieveCurrentInstallation)
                    {
                        LogTextBox("Getting Air Assets from " + AirLocation);
                        Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                        {
                            CurrentProgressLabel.Content = "Copying Air Assets";
                        }));
                        AirVersion = patcher.GetCurrentAirInstall(AirLocation);
                        LogTextBox("Retrieved currently installed Air Assets");
                        LogTextBox("Current Air Assets Version: " + AirVersion);
                    }
                }

                if (AirVersion != LatestAIR)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                    {
                        CurrentProgressLabel.Content = "Retrieving Air Assets";
                    }));
                }

                #endregion

#endif

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

        private void LogTextBox(string s)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
            {
                PatchTextBox.Text += "[" + DateTime.Now.ToShortTimeString() + "] " + s + Environment.NewLine;
            }));
        }

        void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)));

            foreach (var directory in Directory.GetDirectories(sourceDir))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
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