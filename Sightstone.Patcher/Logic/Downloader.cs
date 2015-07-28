using System;
using System.Collections.Generic;
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
        public void DownloadMultipleFiles(List<DownloadFile> filesToDownlad)
        {
            using (var client = new WebClient())
            {
                foreach (var fileDlInfo in filesToDownlad)
                {
                    if (!Directory.Exists(fileDlInfo.OutputPath))
                        Directory.CreateDirectory(fileDlInfo.OutputPath);
                    client.DownloadFileAsync(fileDlInfo.DownloadUri, fileDlInfo.OutputPath);
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
        public string OutputPath { get; set; }

        /// <summary>
        /// If set to false, the if the file with the same name exists, the file will not be downloaded
        /// </summary>
        public bool OverrideFiles { get; set; }
    }
}
