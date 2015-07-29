using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Sightstone.Patcher.Logic
{
    /// <summary>
    /// Downloads multiple files
    /// </summary>
    public class Downloader
    {
        public void DownloadMultipleFiles(List<DownloadFile> filesToDownlad, ProgressBar outProgressBar, Action callbackAction)
        {
            foreach (var fileDlInfo in filesToDownlad)
            {
                foreach (var paths in fileDlInfo.OutputPath.Where(paths => !Directory.Exists(paths)))
                    Directory.CreateDirectory(paths);

                using (var client = new WebClient())
                {
                    client.DownloadFileAsync(fileDlInfo.DownloadUri, fileDlInfo.OutputPath[0]);
                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        Client.RunOnUIThread((() =>
                        {
                            var bytesIn = double.Parse(e.BytesReceived.ToString());
                            var totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                            var percentage = bytesIn / totalBytes * 100;
                            outProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString(CultureInfo.CurrentCulture));
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            if (bytesIn == totalBytes)
                            {

                                callbackAction.Invoke();
                                if (File.Exists(Path.Combine(Client.ExecutingDirectory, "PatchData", "LC_LOL.Version")))
                                {
                                    File.Delete(Path.Combine(Client.ExecutingDirectory, "PatchData", "LC_LOL.Version"));
                                }

                                using (var file = File.Create(Path.Combine(Client.ExecutingDirectory, "PatchData", "LC_LOL.Version")))
                                {
                                    //latestAir = LeagueDownloadLogic.GetLolClientVersion(Client.Reg)
                                    //file.Write(encoding.GetBytes(latestAir), 0,
                                    //encoding.GetBytes(latestAir).Length);
                                }
                            }
                        }));
                    };
                }
            }
        }
    }

    /// <summary>
    /// Used to tell the downloader what files to download
    /// </summary>
    public class DownloadFile
    {
        /// <summary>
        /// The uri of the file to be downloaded
        /// </summary>
        public Uri DownloadUri { get; set; }

        /// <summary>
        /// The output path of the file
        /// </summary>
        public string[] OutputPath { get; set; }

        /// <summary>
        /// If set to false, the if the file with the same name exists, the file will not be downloaded
        /// </summary>
        public bool OverrideFiles { get; set; }
    }
}
