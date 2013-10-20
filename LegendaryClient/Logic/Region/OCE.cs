using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class OCE : BaseRegion
    {
        public override string RegionName
        {
            get { return "OCE"; }
        }

        public override string InternalName
        {
            get { return "OC1"; }
        }

        public override string ChatName
        {
            get { return "oc1"; }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.OCE; }
        }

        public override IPAddress[] PingAddresses
        {
            get
            {
                return new IPAddress[]
                {
                    //No known IP address
                };
            }
        }

        //No known spectator link, returning NA
        public override Uri SpectatorLink
        {
            get { return new Uri("http://spectator.na.lol.riotgames.com/observer-mode/rest/featured"); }
        }

        public override Uri SpectatorIpAddress
        {
            get { return new Uri("216.133.234.17:8088"); }
        }
    }
}
