#region

using System;
using System.Runtime.ExceptionServices;
using System.Threading;

#endregion

namespace LCLog
{
    /// <summary>
    ///     Worlds most basic logger for LegendaryClient
    /// </summary>
    public class Log
    {
        /// <summary>
        ///     Write to the log file on a crash
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            try
            {
                if (e.Exception.Message.Contains("too small for an Int32") ||
                    e.Exception.Message.Contains("Constructor on type "))
                    return;
            }
            catch
            {
                WriteToLog.Log("Failed to write error in FirstChanceException.");
                return;
            }
            WriteToLog.Log("A first chance exception was thrown", "EXCEPTION");
            WriteToLog.Log(e.Exception.Message, "EXCEPTION");
            WriteToLog.Log(e.Exception.StackTrace, "EXCEPTION");
        }

        public static void AppDomain_CurrentDomain(object sender, UnhandledExceptionEventArgs x)
        {
            WriteToLog.Log("An unhandled exception was thrown", "UNHANDLEDEXCEPTION");
            var ex = (Exception) x.ExceptionObject;
            WriteToLog.Log(ex.Message, "UNHANDLEDEXCEPTION");
            WriteToLog.Log(ex.StackTrace, "UNHANDLEDEXCEPTION");
        }

        public static void Application_ThreadException(object sender, ThreadExceptionEventArgs t)
        {
            WriteToLog.Log("An unhandled exception was thrown", "ThreadException");
            WriteToLog.Log(t.Exception.Message, "ThreadException");
            WriteToLog.Log(t.Exception.StackTrace, "ThreadException");
        }
    }
}