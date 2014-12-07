#region

using System;
using System.Net;

#endregion

namespace LegendaryClient.Logic.Patcher
{
    public class LegendaryClientPatcher
    {
        /// <summary>
        ///     Get The Latest LegendaryClient Version
        /// </summary>
        /// <returns></returns>
        public static string GetLatestLCVersion()
        {
            string version = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/Version");
            string latestVersion = version.Split(new[] {Environment.NewLine}, StringSplitOptions.None)[0];
            return latestVersion;
        }

        /// <summary>
        ///     Gets the Download Link For LegendaryClient
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadLocation()
        {
            string downloadLink =
                new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/downloadLink");
            return "https://github.com/eddy5641/LegendaryClient/releases/download/" + downloadLink;
        }
    }
}