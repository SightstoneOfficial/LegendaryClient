using System;

namespace LegendaryClient.Patcher.Logic.Region
{
    class BR : MainRegion
    {
        public override string RegionName
        {
            get { return "BR"; }
        }

        public override string[] Locals
        {
            get { return new[] { "" }; }
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
                return new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/{0}/solutionmanifest");
            }
        }

        public override Uri ReleaseListingUri
        {
            get
            {
                return new Uri("http://l3cdn.riotgames.com/releases/live/projects/lol_air_client/releases/releaselisting_BR");
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
