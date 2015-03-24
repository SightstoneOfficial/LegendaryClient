using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public abstract class BaseRegion
    {
        public abstract string RegionName { get; }

        public abstract bool Garena { get; }

        public abstract string InternalName { get; }

        public abstract string ChatName { get; }

        public abstract Uri NewsAddress { get; }

        public abstract string Locale { get; }

        public abstract string Server { get; }

        public abstract string LoginQueue { get; }

        public abstract IPAddress[] PingAddresses { get; }

        public abstract Uri SpectatorLink { get; }

        public abstract string SpectatorIpAddress { get; set; }

        public abstract string Location { get; }

        public static BaseRegion GetRegion(string requestedRegion)
        {
            requestedRegion = requestedRegion.ToUpper();
            var t = Type.GetType("LegendaryClient.Logic.Region." + requestedRegion);

            if (t != null)
                return (BaseRegion) Activator.CreateInstance(t);
            t = Type.GetType("LegendaryClient.Logic.Region.Garena." + requestedRegion);

            if (t != null)
                return (BaseRegion) Activator.CreateInstance(t);

            return null;
        }
    }
}