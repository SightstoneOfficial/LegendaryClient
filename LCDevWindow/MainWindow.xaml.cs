using LCDevWindow.Commands;
using LCDevWindow.Commands.LegendaryClient;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace LCDevWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        bool pipe = true;
        Timer shutdown = new Timer();
        int shutdownint = 0;
        public MainWindow()
        {
            InitializeComponent();
            Main.win = this;
            //191537514598135486vneaoifjidafd are just random chars, they will match up to the one in LC
            Log("LegendaryClient Logger. Starting Pipe, please wait.", Brushes.Brown);
            Main.inPipeClient = new NamedPipeClientStream(".", "LegendaryClientPipe@191537514598135486vneaoifjidafd", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            Main.inPipeClient.Connect();
            Main.inPipeStream = new StreamString(Main.inPipeClient);
            Log("Pipe to LegendaryClient Created! Logging has started", Brushes.Green);
            Thread xls = new Thread(() =>
                {
                    while (pipe)
                    {
                        string x = Main.inPipeStream.ReadString();

                        if (x == "191537514598135486vneaoifjidafd")
                        {
                            pipe = false;
                            Log("LegendaryClient has closed and the pipe has been shut down!", Brushes.Red);
                            Log("This window will now close in 30 seconds, do \"-abortShutdown\" to stop the shutdown", Brushes.Red);
                            shutdown.Interval = 1000;
                            shutdownint = 0;
                            shutdown.Elapsed += (A, B) =>
                                {
                                    shutdownint++;
                                    if (shutdownint == 30)
                                        Environment.Exit(0);
                                    else if (!((decimal)shutdownint / 5).ToString().Contains("."))
                                        Log("Shutdown in " + (30 - shutdownint) + " seconds", Brushes.OrangeRed);
                                    if (shutdownint == 30)
                                        Log("Shutting down... Please wait", Brushes.OrangeRed);
                                };
                            shutdown.Start();
                            Main.inPipeClient.Close();
                        }
                        else if (x == "AwaitStart")
                        {

                            StartPipe();
                            Log("Starting another pipe to LegendaryClient for sending data", Brushes.Blue);
                        }
                        else if (!x.ToLower().Contains("exception"))
                            Log(x, Brushes.Orange);
                        else if (x.ToLower().Contains("unhandled"))
                            Log(x, Brushes.Red);
                        else Log(x, Brushes.Green);
                    }
                });
            xls.Start();
        }
        private static int numThreads = 4;
        public static void StartPipe()
        {
            int i = 0;
            Thread[] servers = new Thread[numThreads];
            if (i < numThreads)
                i++;
            servers[i] = new Thread(ServerThread);
            servers[i].Start();
        }
        private static void ServerThread(object data)
        {
            NamedPipeServerStream pipeServer =
                new NamedPipeServerStream("LegendaryClientPipe@191537514598135486vneaoifjidafdOUTPUT", PipeDirection.InOut, numThreads);
            int threadId = Thread.CurrentThread.ManagedThreadId;
            pipeServer.WaitForConnection();
            try
            {
                Main.SendPIPE = new StreamString(pipeServer);
                Main.SendPIPE.WriteString("Sender Started");
            }
            catch
            {
            }
        }
        public void Log(string text, SolidColorBrush color)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var tr = new TextRange(LogWindow.Document.ContentEnd, LogWindow.Document.ContentEnd);
                    tr.Text = text + Environment.NewLine;
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                    LogWindow.ScrollToEnd();
                }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string m = DevCommand.Text;
            if (m == "-abortShutdown")
                shutdown.Stop();
            if (m.Contains("(") && m.Contains(")"))
            {
                DevCommand.Text = "";
                string[] tempsplit = m.Split('(');

                object x = Command.GetCommand(tempsplit[0]);
                if (x != null)
                {
                    List<String> splittwo = new List<String>();

                    string[] xm = tempsplit[1].Replace(")", "").Split(',');
                    
                    foreach (string xd in xm)
                        splittwo.Add(xd);
                    if (!x.GetType().ToString().Contains("LCDevWindow.Commands.LegendaryClient"))
                        ((Command)x).ActivateCommand(splittwo.ToArray());
                    else if (x.GetType().ToString().Contains("LCDevWindow.Commands.LegendaryClient"))
                        ((LCCommand)x).ActivateCommand(splittwo.ToArray());
                }
                else
                {
                    Log("Invalid Command! Check out the help tips by doing \"Help()\" (Capitals Matter!!!)", Brushes.Red);
                }
            }
            else
            {
                Log("Invalid Command! Check out the help tips by doing \"Help()\" (Capitals Matter!!!)", Brushes.Red);
            }
       } 
        public class StreamString
        {
            private Stream ioStream;
            private UnicodeEncoding streamEncoding;

            public StreamString(Stream ioStream)
            {
                this.ioStream = ioStream;
                streamEncoding = new UnicodeEncoding();
            }

            public string ReadString()
            {
                try
                {
                    int len;
                    len = ioStream.ReadByte() * 256;
                    len += ioStream.ReadByte();
                    byte[] inBuffer = new byte[len];
                    ioStream.Read(inBuffer, 0, len);

                    return streamEncoding.GetString(inBuffer);
                }
                catch
                {
                    return "191537514598135486vneaoifjidafd";
                }
            }

            public int WriteString(string outString)
            {
                byte[] outBuffer = streamEncoding.GetBytes(outString);
                int len = outBuffer.Length;
                if (len > UInt16.MaxValue)
                {
                    len = (int)UInt16.MaxValue;
                }
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();

                return outBuffer.Length + 2;
            }
        }
    }
}