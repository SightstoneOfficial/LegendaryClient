// ReSharper disable InconsistentNaming
#region

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using ComponentAce.Compression.Libs.zlib;
using LegendaryClient.Patcher.Logic.Region;

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
        public string[] GetLolClientSlnVersion(MainRegion Region)
        {
            //Get the GameClientSln version
            using (new WebClient())
            {
                ReleaseListing = new WebClient().DownloadString(Region.GameReleaseListingUri);
            }

            return ReleaseListing.Split(new[] {Environment.NewLine}, StringSplitOptions.None).Skip(1).ToArray();
        }

        public string GetLatestLCLOLVersion()
        {
            if (File.Exists(Path.Combine(Client.ExecutingDirectory, "LC_LOL.Version")))
            {
                return File.ReadAllText(Path.Combine(Client.ExecutingDirectory, "LC_LOL.Version")).Split( new []{Environment.NewLine}, StringSplitOptions.None)[0];
            }
            var encoding = new ASCIIEncoding();
            File.Create(Path.Combine(Client.ExecutingDirectory, "LC_LOL.Version")).Write(encoding.GetBytes("0.0.0.0"), 0, encoding.GetBytes("0.0.0.0").Length);
            return "0.0.0.0";
        }

        /// <summary>
        ///     Gets the SolutionManifest
        /// </summary>
        /// <returns>
        ///     The SolutionManifest file from riot
        /// </returns>
        public string CreateConfigurationManifest(MainRegion Region)
        {
            string latestSlnVersion = Convert.ToString(GetLolClientSlnVersion(Region));
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