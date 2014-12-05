#region

using System;
using System.IO;
using System.Linq;
using System.Net;
using ComponentAce.Compression.Libs.zlib;

#endregion

namespace LegendaryClient.Patcher.Logic
{
    internal class LeagueDownloadLogic
    {
        public static string ReleaseListing = "";
        public static string SolutionManifest = "";


        /// <summary>
        ///     Gets the Latest LeagueOfLegends lol_game_client_sln version
        /// </summary>
        public string[] GetLolClientSlnVersion()
        {
            //Get the GameClientSln version
            using (new WebClient())
            {
                ReleaseListing =
                    new WebClient().DownloadString(
                        "http://l3cdn.riotgames.com/releases/live/solutions/lol_game_client_sln/releases/releaselisting_NA");
            }

            return ReleaseListing.Split(new[] {Environment.NewLine}, StringSplitOptions.None).Skip(1).ToArray();
        }

        /// <summary>
        ///     Gets the SolutionManifest
        /// </summary>
        /// <returns>
        ///     The SolutionManifest file from riot
        /// </returns>
        public string CreateConfigurationManifest()
        {
            string latestSlnVersion = Convert.ToString(GetLolClientSlnVersion());
            //Get GameClient Language files
            using (new WebClient())
            {
                SolutionManifest =
                    new WebClient().DownloadString(
                        "http://l3cdn.riotgames.com/releases/live/solutions/lol_game_client_sln/releases/" +
                        latestSlnVersion + "/solutionmanifest");
            }
            return SolutionManifest;
        }

        public static void DecompressFile(string inFile, string outFile)
        {
            int data;
            const int stopByte = -1;
            var outFileStream = new FileStream(outFile, FileMode.Create);
            var inZStream = new ZInputStream(File.Open(inFile, FileMode.Open, FileAccess.Read));
            while (stopByte != (data = inZStream.Read()))
            {
                var dataByte = (byte) data;
                outFileStream.WriteByte(dataByte);
            }

            inZStream.Close();
            outFileStream.Close();
        }
    }
}