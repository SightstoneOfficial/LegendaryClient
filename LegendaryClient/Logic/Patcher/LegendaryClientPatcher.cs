using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.Patcher
{
    public class LegendaryClientPatcher
    {
        public LegendaryClientPatcher()
        {

        }

        /// <summary>
        /// Get The Latest LegendaryClient Version
        /// </summary>
        /// <returns></returns>
        public static string GetLatestLCVersion()
        {
            var version = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/Version");
            var LatestVersion = version.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)[0];
            return LatestVersion;
        }

        /// <summary>
        /// Gets the Download Link For LegendaryClient
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadLocation()
        {
            var downloadLink = new WebClient().DownloadString("http://eddy5641.github.io/LegendaryClient/downloadLink");
            return "https://github.com/eddy5641/LegendaryClient/releases/download/" + downloadLink;
        }
    }
}
