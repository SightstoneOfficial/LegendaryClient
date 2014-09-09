using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCLog
{
    /// <summary>
    /// Worlds most basic logger for LegendaryClient
    /// </summary>
    public class Log
    {
        /// <summary>
        /// Write to the log file on a crash
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            if (e.Exception.Message.Contains("too small for an Int32") || e.Exception.Message.Contains("Constructor on type "))
                return;
            WriteToLog.Log("A first chance exception was thrown", "EXCEPTION");
            WriteToLog.Log(e.Exception.Message, "EXCEPTION");
            WriteToLog.Log(e.Exception.StackTrace, "EXCEPTION");
        }
    }
}
