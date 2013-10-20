using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class KR : BaseRegion
    {
        public override string RegionName
        {
            get { return "KR"; }
        }

        public override string InternalName
        {
            get { return "KR1"; }
        }

        public override string ChatName
        {
            get { return "kr"; }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.KR; }
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

        public override Uri SpectatorLink
        {
            get { return new Uri("http://QFKR1PROXY.kassad.in:8088/observer-mode/rest/featured"); }
        }

        public override Uri SpectatorIpAddress
        {
            get { return new Uri("110.45.191.11:80"); }
        }
    }
}
