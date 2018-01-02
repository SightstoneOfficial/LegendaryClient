using System;
using System.Net;

namespace Sightstone.Logic.Patcher
{
    public class SightstonePatcher
    {
        /// <summary>
        ///     Get The Latest Sightstone Version
        /// </summary>
        /// <returns></returns>
        public static string GetLatestLCVersion()
        {
            string version = new WebClient().DownloadString("http://eddy5641.github.io/Sightstone/Version");
            string latestVersion = version.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
            return latestVersion;
        }

        /// <summary>
        ///     Gets the Download Link For Sightstone
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadLocation()
        {
            string downloadLink =
                new WebClient().DownloadString("http://eddy5641.github.io/Sightstone/downloadLink");
            return "https://github.com/eddy5641/Sightstone/releases/download/" + downloadLink;
        }
    }
}