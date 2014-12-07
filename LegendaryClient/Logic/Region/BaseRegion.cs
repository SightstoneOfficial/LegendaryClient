#region

using System;
using System.Net;

#endregion

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

        public abstract PVPNetConnect.Region PVPRegion { get; }

        public abstract IPAddress[] PingAddresses { get; }

        public abstract Uri SpectatorLink { get; }

        //ONLY SET FOR CS SERVER
        public abstract string SpectatorIpAddress { get; set; }

        public abstract string Location { get; }

        public static BaseRegion GetRegion(String requestedRegion)
        {
            requestedRegion = requestedRegion.ToUpper();
            Type t = Type.GetType("LegendaryClient.Logic.Region." + requestedRegion);

            if (t != null)
                return (BaseRegion) Activator.CreateInstance(t);
            t = Type.GetType("LegendaryClient.Logic.Region.Garena." + requestedRegion);

            if (t != null)
                return (BaseRegion) Activator.CreateInstance(t);

            return null;
        }
    }
}