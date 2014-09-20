using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCLog
{
    /// <summary>
    /// Worlds most basic logger for LegendaryClient
    /// </summary>
    public class WriteToLog
    {
        /// <summary>
        /// Where to put the log file
        /// </summary>
        public static string ExecutingDirectory;

        /// <summary>
        /// What is the Log file name
        /// </summary>
        public static string LogfileName;

        /// <summary>
        /// Do the log
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="type"></param>
        public static void Log(String lines, String type = "LOG")
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter(Path.Combine(ExecutingDirectory, "Logs", LogfileName), true);
            file.WriteLine(string.Format("({0} {1}) [{2}]: {3}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), type.ToUpper(), lines));
            file.Close();
        }

        public static void CreateLogFile()
        {
            if (!Directory.Exists(Path.Combine(ExecutingDirectory, "Logs")))
                Directory.CreateDirectory(Path.Combine(ExecutingDirectory, "Logs"));
            if (File.Exists(Path.Combine(ExecutingDirectory, "Logs", LogfileName)))
            {
                File.Delete(Path.Combine(ExecutingDirectory, "Logs", LogfileName));
            }
            LogfileName = LogfileName + "(" + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ")";
            var file = File.Create(Path.Combine(ExecutingDirectory, "Logs", LogfileName));
            file.Close();
        }
    }
}
