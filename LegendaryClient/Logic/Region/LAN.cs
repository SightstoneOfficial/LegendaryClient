using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class LA1 : BaseRegion
    {
        public override string RegionName
        {
            get { return "LA1"; }
        }

        public override string InternalName
        {
            get { return "LA1"; }
        }

        public override string ChatName
        {
            get { return "la1"; }
        }

        public override Uri NewsAddress
        {
            //Unknown
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/la1/es_MX.js"); }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.LA1; }
        }

        public override IPAddress[] PingAddresses
        {
            get
            {
                return new IPAddress[]
                {
                    IPAddress.Parse("66.151.33.33"),

                };
            }
        }

        public override Uri SpectatorLink
        {
            get { return new Uri("http://spectator.la1.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            //Unknown
            get { return ""; }
        }
    }
}
