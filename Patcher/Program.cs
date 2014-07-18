using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.IO;
using System.Net;

namespace Patcher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string LaunchLoaction = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            WriteToConsole("LegendaryClient Updater");
            WebClient client = new WebClient();
            string VersionString = "";
            try
                {

                    VersionString = client.DownloadString(new Uri("http://eddy5641.github.io/LegendaryClient/filename.html"));

                    string[] VersionSplit = VersionString.Split('|');

                    WriteToConsole("Update data: " + VersionSplit[0]);
                }
                catch
                {
                    WriteToConsole("[CRITICAL]: Unable To Retreive Client Version Name");
                }
            Stream inStream = File.OpenRead(Path.Combine("temp", "1.0.1.2.zip"));
            WriteToConsole("Starting To Extract LegendaryClient from:");

            using (var gzipStream = new GZipInputStream(inStream))
            {
                TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
                tarArchive.ExtractContents("temp");
                tarArchive.Close();
            }
            WriteToConsole("LegendaryClient Is Now Extracted. Closing This window and launching LegendaryClient");
            System.Diagnostics.Process.Start("LegendaryClient.exe");
            Environment.Exit(0);
        }
        private static void WriteToConsole(string Text)
        {
            Console.WriteLine(Text);
        }
    }
}