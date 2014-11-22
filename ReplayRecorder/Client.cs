using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReplayRecorder
{
    public class Client
    {
        /// <summary>
        /// Gets the server the recoder is recording on
        /// </summary>
        internal static string Server;

        /// <summary>
        /// The game ID of the recorded game
        /// </summary>
        internal static int GameId;

        /// <summary>
        /// The region the recorded game is on
        /// </summary>
        internal static string Region;

        /// <summary>
        /// The key to the recorded game
        /// </summary>
        internal static string Key;

        /// <summary>
        /// Where this program was started from (LegendaryClient)
        /// </summary>
        internal static string ExecutingDirectory;
    }
}
