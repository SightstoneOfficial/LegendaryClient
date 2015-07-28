using System;
using System.Net;

namespace Sightstone.Patcher.Logic.Region
{
    class PBE : MainRegion
    {
        public override string RegionName
        {
            get { return "PBE"; }
        }

        public override string[] Locals
        {
            get { return new[] { "en_US" }; }
        }

        public override RegionType RegionType
        {
            get
            {
                return RegionType.PBE;
            }
        }

        public override Uri ClientUpdateUri
        {
            get
            {
                var x = new WebClient().DownloadString(ReleaseListingUri).Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                return new Uri(string.Format("http://l3cdn.riotgames.com/releases/pbe/projects/lol_air_client/releases/{0}/packages/files/packagemanifest", x));
            }
        }

        public override Uri ReleaseListingUri
        {
            get
            {
                return new Uri("http://l3cdn.riotgames.com/releases/pbe/projects/lol_air_client/releases/releaselisting_" + RegionName);
            }
        }

        public override Uri GameClientReleaseListingUri
        {
            get
            {
                return new Uri("http://l3cdn.riotgames.com/releases/pbe/projects/lol_game_client/releases/releaselisting_" + RegionName);
            }
        }

        public override Uri GameClientUpdateUri
        {
            get
            {
                var x = new WebClient().DownloadString(GameClientReleaseListingUri).Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                return new Uri(string.Format("http://l3cdn.riotgames.com/releases/pbe/projects/lol_game_client/releases/{0}/packages/files/packagemanifest", x));
            }
        }

        public override Uri GameReleaseListingUri
        {
            get
            {
                return new Uri("http://l3cdn.riotgames.com/releases/pbe/solutions/lol_game_client_sln/releases/releaselisting_" + RegionName);
            }
        }
    }
}