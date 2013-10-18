using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class RU : BaseRegion
    {
        public override string RegionName
        {
            get { return "RU"; }
        }

        public override string InternalName
        {
            get { return "RU1"; }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.RU; }
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
            get { return new Uri("http://spectator.ru.lol.riotgames.com/observer-mode/rest/featured"); }
        }

        public override Uri SpectatorIpAddress
        {
            get { return new Uri("95.172.65.242:80"); }
        }
    }
}
