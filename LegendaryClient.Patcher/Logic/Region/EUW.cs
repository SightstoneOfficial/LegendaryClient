using System;

namespace LegendaryClient.Patcher.Logic.Region
{
    class EUW : MainRegion
    {
        public override string RegionName
        {
            get { return ""; }
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
                return new Uri("");
            }
        }

        public override Uri ReleaseListingUri
        {
            get
            {
                return new Uri("");
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
