using System;
using System.IO;
using System.Linq;
using System.Net;

namespace Sightstone.Logic.Patcher
{
    public class DownloadAir
    {
        /// <summary>
        ///     Easy way to download the client
        /// </summary>
        /// <param name="mainDownloadLocation">Where is the base that you want to put the file</param>
        public static void EasyDownloader(string mainDownloadLocation)
        {
            string latestVersion = GetLatestVersion();
            string[] releaseManifast = GetReleaseManifast(latestVersion);
            DownloadAirFix(releaseManifast, mainDownloadLocation);
        }

        /// <summary>
        ///     Retreives the latest lol_air_client version
        /// </summary>
        /// <returns>The latest air client version</returns>
        public static string GetLatestVersion()
        {
            string latestAirList =
                new WebClient().DownloadString(
                    "http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_NA");
            string[] latestAir = latestAirList.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            return latestAir[0];
        }

        /// <summary>
        ///     Gets the list of files to be downloaded
        /// </summary>
        /// <param name="latestVersion">The latest lol_air_client version</param>
        /// <returns>The list of files that need to be downloaded</returns>
        public static string[] GetReleaseManifast(string latestVersion)
        {
            string package =
                new WebClient().DownloadString(
                    "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/" + latestVersion +
                    "/packages/files/packagemanifest");
            string[] fileMetaData =
                package.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Skip(1).ToArray();
            return fileMetaData;
        }

        public static void DownloadAirFix(string[] fileMetaData, string downloadOutputLocation)
        {
            //Invalid specified output folder. Specify a valid location
            if (!Directory.Exists(downloadOutputLocation))
                return;
        }
    }
}