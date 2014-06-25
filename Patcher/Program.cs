using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;

namespace Patcher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("LegendaryClient Updater");
            Stream inStream = File.OpenRead(Path.Combine("temp", "1.0.1.2.zip"));

            using (GZipInputStream gzipStream = new GZipInputStream(inStream))
            {
                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                tarArchive.ExtractContents("temp");
                tarArchive.CloseArchive();
            }
            System.Diagnostics.Process.Start("LegendaryClient.exe");
            Environment.Exit(0);
        }
    }
}