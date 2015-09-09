using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Sightstone.Patcher.Logic
{
    /// <summary>
    /// Downloads multiple files
    /// </summary>
    public class Downloader
    {
        private readonly List<DownloadFile> _finished = new List<DownloadFile>();

        public delegate void OnFinished();
        public event OnFinished OnFinishedDownloading;

        public delegate void OnProgressChanged(double downloaded, double toDownload);
        public event OnProgressChanged OnDownloadProgressChanged;

        private long _downloadedBytes = 0;
        private long _bytesToDownload;

        private volatile int _webClientToCreate;
        public async void DownloadMultipleFiles(List<DownloadFile> filesToDownlad)
        {
            _webClientToCreate = Client.MaximumWebClient;
            //_downloading = filesToDownlad;
            _bytesToDownload = filesToDownlad.Sum(x => x.FileSize);
            foreach (var fileDlInfo in filesToDownlad)
            {
                foreach (var paths in fileDlInfo.OutputPath.Where(paths => !Directory.Exists(Path.GetDirectoryName(paths))))
                    Directory.CreateDirectory(Path.GetDirectoryName(paths));

                var dlInfo = fileDlInfo;
                
                if (File.Exists(dlInfo.OutputPath[0]))
                {
                    if (new FileInfo(dlInfo.OutputPath[0]).Length == dlInfo.FileSize)
                    {
                        Client.RunOnUIThread((() =>
                        {
                            var change = dlInfo.FileSize;
                            _downloadedBytes = _downloadedBytes + change;
                            OnDownloadProgressChanged?.Invoke(_downloadedBytes, _bytesToDownload);
                        }));
                    }
                    continue;
                }

                using (var client = new WebClient())
                {
                    while (_webClientToCreate == 0)
                    {
                        await Task.Delay(10);
                    }
                    _webClientToCreate--;

                    if (fileDlInfo.DownloadUri.ToString().EndsWith(".compressed"))
                    {

                        var info = fileDlInfo;
                        client.DownloadDataCompleted += (sender, e) =>
                        {
                            _webClientToCreate++;
                            var uncompressed = ZlibStream.UncompressBuffer(e.Result);
                            try
                            {
                                foreach (var outputs in info.OutputPath)
                                    File.WriteAllBytes(outputs.Replace(".compressed", string.Empty), uncompressed);
                            }
                            catch
                            {
                            }
                            _finished.Add(info);
                            ((WebClient)sender).Dispose();
                        };
                        client.DownloadDataAsync(fileDlInfo.DownloadUri);
                    }
                    else
                    {
                        var info = fileDlInfo;
                        client.DownloadFileCompleted += (sender, e) =>
                        {
                            _finished.Add(info);
                            if (info.OutputPath.Count() != 1)
                            {
                                foreach (var outputs in info.OutputPath.Skip(1))
                                {
                                    File.Copy(info.OutputPath[0], outputs, info.OverrideFiles);
                                }
                            }
                            _webClientToCreate++;
                        };
                        client.DownloadFileAsync(fileDlInfo.DownloadUri, fileDlInfo.OutputPath[0]);
                    }

                    long lastbytes = 0;
                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        Client.RunOnUIThread((() =>
                        {
                            var bytesIn = e.BytesReceived;
                            var change = bytesIn - lastbytes;
                            _downloadedBytes = _downloadedBytes + change;
                            OnDownloadProgressChanged?.Invoke(_downloadedBytes, _bytesToDownload);
                            lastbytes = bytesIn;
                        }));
                    };
                }
            }
            if (_finished.Count != 0)
            {
                while (_webClientToCreate != Client.MaximumWebClient)
                    await Task.Delay(10);
                OnFinishedDownloading?.Invoke();
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
        /// The size of the file
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// If set to false, the if the file with the same name exists, the file will not be downloaded
        /// </summary>
        public bool OverrideFiles { get; set; }
    }
}
