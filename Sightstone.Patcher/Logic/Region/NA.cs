using System;
using System.Net;

namespace Sightstone.Patcher.Logic.Region
{
    // ReSharper disable once InconsistentNaming
    internal class NA : MainRegion
    {
        public override string RegionName
        {
            get { return "NA"; }
        }

        public override string[] Locals
        {
            get { return new[] { "en_US" }; }
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
                return new Uri(
                    $"http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/{x}/packages/files/packagemanifest");
            }
        }

        public override Uri ReleaseListingUri => new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_" + RegionName);

        public override Uri GameClientReleaseListingUri => new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_" + RegionName);

        public override Uri GameClientUpdateUri
        {
            get
            {
                var x = new WebClient().DownloadString(GameClientReleaseListingUri).Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                return new Uri($"http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/{x}/packages/files/packagemanifest");
            }
        }

        public override Uri GameReleaseListingUri => new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_" + RegionName);

        public  Uri GameSlnReleaseListingUri => new Uri("http://l3cdn.riotgames.com/releases/live/solutions/lol_game_client_sln/releases/releaselisting_" + RegionName);
    }
}