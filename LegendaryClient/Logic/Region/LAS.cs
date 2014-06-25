using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class LAS : BaseRegion
    {
        public override string RegionName
        {
            get { return "LA2"; }
        }

        public override string InternalName
        {
            get { return "LA2"; }
        }

        public override string ChatName
        {
            get { return "la2"; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/la2/es_MX.js"); }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.LA2; }
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
            get { return new Uri("http://spectator.la2.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "66.151.33.19:80"; }
        }
    }
}