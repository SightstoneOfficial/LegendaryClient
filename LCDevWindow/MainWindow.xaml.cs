using LCDevWindow.Commands;
using LCDevWindow.Commands.Sightstone;
using System;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using Timer = System.Timers.Timer;

namespace LCDevWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        bool _pipe = true;
        readonly Timer _shutdown = new Timer();
        int _shutdownint;

        public MainWindow()
        {
            InitializeComponent();
            Main.win = this;
            //191537514598135486vneaoifjidafd are just random chars, they will match up to the one in LC
            Log("Sightstone Logger. Starting Pipe, please wait.", Brushes.Brown);
            Main.inPipeClient = new NamedPipeClientStream(".", "SightstonePipe@191537514598135486vneaoifjidafd", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            Main.inPipeClient.Connect();
            Main.inPipeStream = new StreamString(Main.inPipeClient);
            Log("Pipe to Sightstone Created! Logging has started", Brushes.Green);
            var xls = new Thread(() =>
                {
                    while (_pipe)
                    {
                        var x = Main.inPipeStream.ReadString();

                        switch (x)
                        {
                            case "191537514598135486vneaoifjidafd":
                                _pipe = false;
                                Log("Sightstone has closed and the pipe has been shut down!", Brushes.Red);
                                Log("This window will now close in 30 seconds, do \"-abortShutdown\" to stop the shutdown", Brushes.Red);
                                _shutdown.Interval = 1000;
                                _shutdownint = 0;
                                _shutdown.Elapsed += (a, b) =>
                                {
                                    _shutdownint++;
                                    if (_shutdownint == 30)
                                        Environment.Exit(0);
                                    else if (!((decimal)_shutdownint / 5).ToString(CultureInfo.CurrentCulture).Contains("."))
                                        Log("Shutdown in " + (30 - _shutdownint) + " seconds", Brushes.OrangeRed);
                                    if (_shutdownint == 30)
                                        Log("Shutting down... Please wait", Brushes.OrangeRed);
                                };
                                _shutdown.Start();
                                Main.inPipeClient.Close();
                                break;
                            case "AwaitStart":

                                StartPipe();
                                Log("Starting another pipe to Sightstone for sending data", Brushes.Blue);
                                break;
                            default:
                                if (!x.ToLower().Contains("exception"))
                                    Log(x, Brushes.Orange);
                                else if (x.ToLower().Contains("unhandled"))
                                    Log(x, Brushes.Red);
                                else Log(x, Brushes.Green);
                                break;
                        }
                    }
                });
            xls.Start();
        }

        private const int NumThreads = 4;

        public static void StartPipe()
        {
            var i = 0;
            var servers = new Thread[NumThreads];
            if (i < NumThreads)
                i++;
            servers[i] = new Thread(ServerThread);
            servers[i].Start();
        }
        private static void ServerThread(object data)
        {
            var pipeServer =
                new NamedPipeServerStream("SightstonePipe@191537514598135486vneaoifjidafdOUTPUT", PipeDirection.InOut, NumThreads);
            //var threadId = Thread.CurrentThread.ManagedThreadId;
            pipeServer.WaitForConnection();
            try
            {
                Main.SendPIPE = new StreamString(pipeServer);
                Main.SendPIPE.WriteString("Sender Started");
            }
            catch
            {
                // ignored
            }
        }
        public void Log(string text, SolidColorBrush color)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new ThreadStart(() =>
                {
                    var tr = new TextRange(LogWindow.Document.ContentEnd, LogWindow.Document.ContentEnd)
                    {
                        Text = text + Environment.NewLine
                    };
                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                    LogWindow.ScrollToEnd();
                }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var m = DevCommand.Text;
            if (m == "-abortShutdown")
            {
                _shutdown.Stop();
                return;
            }
            if (m.Contains("(") && m.Contains(")"))
            {
                DevCommand.Text = "";
                var tempsplit = m.Split('(');

                var x = Command.GetCommand(tempsplit[0]);
                if (x != null)
                {
                    var xm = tempsplit[1].Replace(")", "").Split(',');

                    var splittwo = xm.ToList();
                    if (!x.GetType().ToString().Contains("LCDevWindow.Commands.Sightstone") && x.GetType().ToString().Contains("LCDevWindow.Commands"))
                        ((Command)x).ActivateCommand(splittwo.ToArray());
                    else if (x.GetType().ToString().Contains("LCDevWindow.Commands.Sightstone"))
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
            private readonly Stream _ioStream;
            private readonly UnicodeEncoding _streamEncoding;

            public StreamString(Stream ioStream)
            {
                _ioStream = ioStream;
                _streamEncoding = new UnicodeEncoding();
            }

            public string ReadString()
            {
                try
                {
                    var len = _ioStream.ReadByte() * 256;
                    len += _ioStream.ReadByte();
                    var inBuffer = new byte[len];
                    _ioStream.Read(inBuffer, 0, len);

                    return _streamEncoding.GetString(inBuffer);
                }
                catch
                {
                    return "191537514598135486vneaoifjidafd";
                }
            }

            public int WriteString(string outString)
            {
                var outBuffer = _streamEncoding.GetBytes(outString);
                var len = outBuffer.Length;
                if (len > ushort.MaxValue)
                {
                    len = ushort.MaxValue;
                }
                _ioStream.WriteByte((byte)(len / 256));
                _ioStream.WriteByte((byte)(len & 255));
                _ioStream.Write(outBuffer, 0, len);
                _ioStream.Flush();

                return outBuffer.Length + 2;
            }
        }
    }
}