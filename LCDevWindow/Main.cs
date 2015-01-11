using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDevWindow
{
    public class Main
    {
        /// <summary>
        /// Wow this is empty :P
        /// </summary>
        public static MainWindow win;

        /// <summary>
        /// Main Pipe used for talking to LC
        /// </summary>
        public static NamedPipeClientStream inPipeClient;

        /// <summary>
        /// The thing that talks to LC
        /// </summary>
        public static MainWindow.StreamString inPipeStream;
        public static MainWindow.StreamString SendPIPE;

        public static Dictionary<String, Object> Vars = new Dictionary<String, Object>();
    }
}
