using System;
using System.Net;

namespace LegendaryClient.Patcher.Logic.Region
{
    class RU : MainRegion
    {
        public override string RegionName
        {
            get { return "RU"; }
        }

        public override string[] Locals
        {
            get { return new[] { "ru_RU" }; }
        }

        public override RegionType RegionType
        {
            get
            {
                return RegionType.Riot;
            }
        }

        public override Uri ClientUpdateUri
        {
            get
            {
                var x = new WebClient().DownloadString(ReleaseListingUri).Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                return new Uri(string.Format("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/{0}/packages/files/packagemanifest", x));
            }
        }

        public override Uri ReleaseListingUri
        {
            get
            {
                return new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_RU");
            }
        }

        public override Uri GameClientUpdateUri
        {
            get
            {
                return new Uri("");
            }
        }

        public override Uri GameReleaseListingUri
        {
            get
            {
                return new Uri("");
            }
        }
    }
}
