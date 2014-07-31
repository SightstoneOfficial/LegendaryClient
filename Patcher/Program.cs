using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Patcher
{
    internal class Program
    {
        private static void Main(string[] args)
        {

            string LaunchLoaction = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            if (File.Exists(Path.Combine(LaunchLoaction, "Patcher.log")))
            {
                File.Delete(Path.Combine(LaunchLoaction, "Patcher.log"));
            }
            WriteToConsole("LegendaryClient Updater Version 1.0.0.0 UNSTABLE");
            Console.Title = "LegendaryClient Updater Version 1.0.0.0";
            WriteToConsole("This program should have been started by LegendaryClient or LegendaryClient.exe");
            WriteToConsole("If this program was not started by LegendaryClient.exe, close this program and launch LegendaryClient");
            WebClient client = new WebClient();
            string VersionString = "";
            bool RetreivedFile = false;
            WriteToConsole("Checking if LegendaryClient Is Running");
            if (Directory.Exists(Path.Combine(LaunchLoaction, "Temp")))
            {
                Directory.Delete(Path.Combine(LaunchLoaction, "Temp"), true);
            }
            Directory.CreateDirectory(Path.Combine(LaunchLoaction, "Temp"));
            try
            {
                KillLegendaryClient();
            }
            catch 
            { 
                //So op code here
            }

            try
            {
                File.Copy(Path.Combine(LaunchLoaction, "Client", "Temp", "LegendaryClientUpdateFile.zip"), Path.Combine(LaunchLoaction, "Temp", "LegendaryClientUpdateFile.zip"));
                RetreivedFile = true;
            }
            catch 
            {
            
            }

            if (Directory.Exists(Path.Combine(LaunchLoaction, "Client")))
            {
                Directory.Delete(Path.Combine(LaunchLoaction, "Client"), true);
            }
            Directory.CreateDirectory(Path.Combine(LaunchLoaction, "Client"));
            try
                {

                    VersionString = client.DownloadString(new Uri("http://eddy5641.github.io/LegendaryClient/filename.html"));

                    string[] VersionSplit = VersionString.Split('|');

                    WriteToConsole("Update data: " + VersionSplit[0]);
                }
                catch
                {
                    WriteToConsole("Unable To Retreive Client Version Name");
                }

            WriteToConsole("Retreving LegendaryClient Update File");

            Copy(Path.Combine(LaunchLoaction, "Client"), LaunchLoaction);

            WriteToConsole("Starting to install update");
            try
            {
                ExtractZipFile(Path.Combine(LaunchLoaction, "Temp", "LegendaryClientUpdateFile.zip"), null, Path.Combine(LaunchLoaction, "Client"));
            }
            catch
            { }
            WriteToConsole("LegendaryClient Is Now Extracted. Closing This window and launching LegendaryClient");

            if (Directory.Exists(Path.Combine(LaunchLoaction, "Temp")))
            {
                Directory.Delete(Path.Combine(LaunchLoaction, "Temp"));
            }
            else
            {
                WriteToConsole("Unable to retreive LegendaryClient Update Files. Please Launch LaunchLegendaryClient");
            }
            System.Diagnostics.Process.Start("LegendaryClient.exe");
            Environment.Exit(0);
        }
        private static void WriteToConsole(string Text, string Type = "Info")
        {
            Console.WriteLine(string.Format("[{0}]: " + Text));
            Log(Text);
            //
        }
        public static void KillLegendaryClient()
        {
            System.Diagnostics.Process[] procs = null;

            try
            {
                procs = Process.GetProcessesByName("LegendaryClient.vshost");

                Process legendaryClientProc = procs[0];

                if (!legendaryClientProc.HasExited)
                {
                    legendaryClientProc.Kill();
                    WriteToConsole("LegendaryClient Was Running. It was terminated");
                }
            }
            finally
            {
                if (procs != null)
                {
                    foreach (Process p in procs)
                    {
                        p.Dispose();
                    }
                }
            }
        }
        public static void ExtractZipFile(string archiveFilenameIn, string password, string outFolder) 
        {
            ZipFile zf = null;
            try 
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password)) 
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf) 
                {
                    if (!zipEntry.IsFile) 
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath)) 
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            } 
            finally 
            {
                if (zf != null) 
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }
        private static void Copy(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);

            foreach (var directory in Directory.GetDirectories(sourceDir))
                Copy(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }


        //Log stuff from here down
        static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (e.Exception.Message.Contains("too small for an Int32") || e.Exception.Message.Contains("Constructor on type "))
                return;
            Log("A first chance exception was thrown", "EXCEPTION");
            Log(e.Exception.Message, "EXCEPTION");
            Log(e.Exception.StackTrace, "EXCEPTION");
        }
        public static void Log(String lines, String type = "LOG")
        {
            string LaunchLoaction = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(LaunchLoaction, "Patcher.log"), true);
            file.WriteLine(string.Format("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), type, lines));
            file.Close();
        }
    }
}