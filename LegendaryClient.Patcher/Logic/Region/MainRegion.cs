// ReSharper disable InconsistentNaming
using System;

namespace LegendaryClient.Patcher.Logic.Region
{
    internal abstract class MainRegion
    {
        public abstract string RegionName { get; }

        public abstract string[] Locals { get; }

        public abstract RegionType RegionType { get; }

        public abstract Uri ClientUpdateUri { get; }

        public abstract Uri ReleaseListingUri { get; }

        public abstract Uri GameClientUpdateUri { get; }

        public abstract Uri GameReleaseListingUri { get; }
    }

    public enum RegionType
    {
        Riot,
        Garena,
        PBE,
        KR,
        CustomServer
    }
}