using ComponentAce.Compression.Libs.zlib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Patcher.Logic
{
    class LeagueDownloadLogic
    {
        public LeagueDownloadLogic()
        {

        }

        public static string releaselisting = "";
        public static string solutionmanifest = "";

        
        /// <summary>
        /// Gets the Latest LeagueOfLegends lol_game_client_sln version
        /// </summary>
        public string[] GetLolClientSlnVersion()
        {
            //Get the GameClientSln version
            using (WebClient client = new WebClient())
            {
                releaselisting = new WebClient().DownloadString("http://l3cdn.riotgames.com/releases/live/solutions/lol_game_client_sln/releases/releaselisting_NA");                            
            }

            return releaselisting.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Skip(1).ToArray();
        }

        /// <summary>
        /// Gets the SolutionManifest
        /// </summary>
        /// <returns>
        /// The SolutionManifest file from riot
        /// </returns>
        public string CreateConfigurationmanifest()
        {
            string LatestSlnVersion = Convert.ToString(GetLolClientSlnVersion());
            //Get GameClient Language files
            using (WebClient client = new WebClient())
            {
                solutionmanifest = new WebClient().DownloadString("http://l3cdn.riotgames.com/releases/live/solutions/lol_game_client_sln/releases/" + LatestSlnVersion + "/solutionmanifest");                
            }
            return solutionmanifest;
        }
        public static void uncompressFile(string inFile, string outFile)
        {
            int data = 0;
            int stopByte = -1;
            System.IO.FileStream outFileStream = new System.IO.FileStream(outFile, System.IO.FileMode.Create);
            ZInputStream inZStream = new ZInputStream(System.IO.File.Open(inFile, System.IO.FileMode.Open, System.IO.FileAccess.Read));
            while (stopByte != (data = inZStream.Read()))
            {
                byte _dataByte = (byte)data;
                outFileStream.WriteByte(_dataByte);
            }

            inZStream.Close();
            outFileStream.Close();
        }
    }
}
