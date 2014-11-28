using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayRecorder
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Unable to record correctly. Please restart the program. Press any key to exit.");
                Console.ReadKey();
            }
            else
            {
                Client.ExecutingDirectory = args[0];
                Client.GameId = Convert.ToInt32(args[1]);
                Client.Key = args[2];
                Client.Region = args[3];
                Client.Server = args[4];

                ReplayRecorder recorder = new ReplayRecorder(Client.Server, Client.GameId, Client.Region, Client.Key);
                recorder.OnGotChunk += (o) => { Console.WriteLine("Received chunk: " + o); };
                recorder.OnReplayRecorded += () => { Console.WriteLine("Replay recorded!"); Console.ReadLine(); };
           }
        }
    }
}
