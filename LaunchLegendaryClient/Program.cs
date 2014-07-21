using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaunchLegendaryClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting LegendaryClient. This window will close automatically");
            string LaunchLoaction = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            System.Diagnostics.Process.Start(Path.Combine("Client", "LegendaryClient.exe"));
            Environment.Exit(0);
        }
    }
}
