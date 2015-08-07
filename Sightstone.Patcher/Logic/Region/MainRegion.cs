// ReSharper disable InconsistentNaming
using System;

namespace Sightstone.Patcher.Logic.Region
{
    internal abstract class MainRegion
    {
        public abstract string RegionName { get; }

        public abstract string[] Locals { get; }

        public abstract RegionType RegionType { get; }

        public abstract Uri ClientUpdateUri { get; }

        public abstract Uri ReleaseListingUri { get; }

        public abstract Uri GameClientReleaseListingUri { get; }

        public abstract Uri GameClientUpdateUri { get; }

        public abstract Uri GameReleaseListingUri { get; }

        public abstract Uri GameSlnReleaseListingUri { get; }

        public static MainRegion GetMainRegion(string Region)
        {
            Region = Region.ToUpper();
            var t = Type.GetType("Sightstone.Patcher.Logic.Region." + Region);
            if (t == null)
                Type.GetType("Sightstone.Patcher.Logic.Region.Garena." + Region);

            return t != null ? (MainRegion)Activator.CreateInstance(t) : null;
        }
    }

    public enum RegionType
    {
        Riot,
        Garena,
        PBE,
        KR
    }
}