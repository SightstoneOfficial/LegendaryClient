using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RiotPatch.RADS.projects.lol_air_client
{
    public class DownloadAir
    {
        /// <summary>
        /// Easy way to download the client
        /// </summary>
        /// <param name="MainDownloadLocation">Where is the base that you want to put the file</param>
        public static void EasyDownloader(string MainDownloadLocation)
        {
            string LatestVersion = GetLatestVersion();
            string[] ReleaseManafast = GetReleaseManafast(LatestVersion);
            DownloadAir(ReleaseManafast, MainDownloadLocation);
        }
        /// <summary>
        /// Retreives the latest lol_air_client version
        /// </summary>
        /// <returns>The latest air client version</returns>
        public static string GetLatestVersion()
        {
            string LatestAirList = new WebClient().DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_NA");
            string[] LatestAir = LatestAirList.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return LatestAir[0];
        }

        /// <summary>
        /// Gets the list of files to be downloaded
        /// </summary>
        /// <param name="LatestVersion">The latest lol_air_client version</param>
        /// <returns>The list of files that need to be downloaded</returns>
        public static string[] GetReleaseManafast(string LatestVersion)
        {
            string Package = new WebClient().DownloadString("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/" + LatestVersion + "/packages/files/packagemanifest");
            string[] FileMetaData = Package.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Skip(1).ToArray();
            return FileMetaData;
        }
        public static void DownloadAir(string[] FileMetaData, string DownloadOutputLocation)
        {
            //Invalid specified output folder. Specify a valid location
            if (!Directory.Exists(DownloadOutputLocation))
                return;

        }
    }
}
