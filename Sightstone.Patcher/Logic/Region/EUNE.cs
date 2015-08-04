using System;
using System.Net;

namespace Sightstone.Patcher.Logic.Region
{
    class EUNE : MainRegion
    {
        public override string RegionName => "EUNE";

        public override string[] Locals => new[] { "en_PL", "pl_PL", "el_GR", "ro_RO", "hu_HU", "cs_CZ" };

        public override RegionType RegionType => RegionType.Riot;

        public override Uri ClientUpdateUri
        {
            get
            {
                var x = new WebClient().DownloadString(ReleaseListingUri).Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                return
                    UriVerify.VerifyUri(
                        new[]
                        {
                            new Uri(
                                string.Format(
                                    "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/{0}/packages/files/packagemanifest",
                                    x[0])),
                            new Uri(
                                string.Format(
                                    "http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/{0}/packages/files/packagemanifest",
                                    x[0]))
                        });
            }
        }

        public override Uri ReleaseListingUri => new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_" + RegionName);

        public override Uri GameClientReleaseListingUri => new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_" + RegionName);

        public override Uri GameClientUpdateUri
        {
            get
            {
                var x = new WebClient().DownloadString(GameClientReleaseListingUri).Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0];
                return new Uri(string.Format("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/{0}/packages/files/packagemanifest", x));
            }
        }

        public override Uri GameReleaseListingUri => new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_game_client/releases/releaselisting_" + RegionName);
        public override Uri GameSlnReleaseListingUri => new Uri("http://l3cdn.riotgames.com/releases/live/solutions/lol_game_client_sln/releases/releaselisting_" + RegionName);
    }
}