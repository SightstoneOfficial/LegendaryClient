using LCDevWindow.Commands;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LCDevWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Main.win = this;
            //191537514598135486vneaoifjidafd are just random chars, they will match up to the one in LC
            Log("LegendaryClient Logger. Starting Pipe, please wait.", Brushes.Yellow);

            Main.pipeClient = new NamedPipeClientStream(".", "LegendaryClientPipe@191537514598135486vneaoifjidafd", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
            StreamString ss = new StreamString(Main.pipeClient);
            Log("Pipe to LegendaryClient Created! Logging has started", Brushes.Gold);
            while (true)
            {
                string x = ss.ReadString();
                if (x == "191537514598135486vneaoifjidafd->RemoveAllPipe[MainWin.Shutdown.AppClose]")
                {
                    Log("LegendaryClient has closed and the pipe has been shut down!", Brushes.Red);
                    Environment.Exit(0);
                }
                Log(x, Brushes.Green);
            }
        }
        public void Log(string text, SolidColorBrush color)
        {
            var tr = new TextRange(LogWindow.Document.ContentEnd, LogWindow.Document.ContentEnd);
            tr.Text = text + Environment.NewLine;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            LogWindow.ScrollToEnd();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string m = DevCommand.Text;
            if (m.Contains("(") && m.Contains(")"))
            {
                DevCommand.Text = "";
                string[] tempsplit = m.Split('(');

                Command x = Command.GetCommand(tempsplit[0]);
                if (x != null)
                {
                    List<String> splittwo = new List<String>();

                    string[] xm = tempsplit[1].Replace(")", "").Split(',');
                    foreach (string xd in xm)
                        splittwo.Add(xd);
                    x.ActivateCommand(splittwo.ToArray());
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
                    return "Wingless Air Client Has Shutdown. Press enter to close";
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