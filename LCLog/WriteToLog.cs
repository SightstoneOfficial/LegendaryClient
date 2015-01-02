#region

using System;
using System.IO;

#endregion

namespace LCLog
{
    /// <summary>
    ///     Worlds most basic logger for LegendaryClient
    /// </summary>
    public class WriteToLog
    {
        /// <summary>
        ///     Where to put the log file
        /// </summary>
        public static string ExecutingDirectory;

        /// <summary>
        ///     What is the Log file name
        /// </summary>
        public static string LogfileName;

        /// <summary>
        ///     Has the disconnected from internet message been sent?
        ///     Resend the message if it hasn't
        /// </summary>
        private static bool Disconnected = false;

        /// <summary>
        ///     Do the log
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="type"></param>
        public static void Log(String lines, String type = "LOG")
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                Disconnected = false;
                using (
                    FileStream stream = File.Open(Path.Combine(ExecutingDirectory, "Logs", LogfileName), FileMode.Append,
                        FileAccess.Write, FileShare.ReadWrite))
                using (var file = new StreamWriter(stream))
                    file.WriteLine("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(),
                        DateTime.Now.ToShortTimeString(), type.ToUpper(), lines);
            }
            else if (!Disconnected)
            {
                Disconnected = true;
                using (
                    FileStream stream = File.Open(Path.Combine(ExecutingDirectory, "Logs", LogfileName), FileMode.Append,
                        FileAccess.Write, FileShare.ReadWrite))
                using (var file = new StreamWriter(stream))
                    file.WriteLine("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(),
                        DateTime.Now.ToShortTimeString(), "NETWORK", "You have disconnected from the internet. Logs have stopped. Please reconnect!");
            }
        }

        public static void CreateLogFile()
        {
            //Generate A Unique file to use as a log file
            if (!Directory.Exists(Path.Combine(ExecutingDirectory, "Logs")))
                Directory.CreateDirectory(Path.Combine(ExecutingDirectory, "Logs"));
            LogfileName = string.Format("{0}T{1} {2}", DateTime.Now.ToShortDateString().Replace("/", "_"),
                DateTime.Now.ToShortTimeString().Replace(" ", "").Replace(":", "-"), "_" + LogfileName);
            FileStream file = File.Create(Path.Combine(ExecutingDirectory, "Logs", LogfileName));
            file.Close();
        }
    }
}