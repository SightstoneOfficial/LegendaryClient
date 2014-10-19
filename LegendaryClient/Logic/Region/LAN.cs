using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.Region
{
    public sealed class LAN : BaseRegion
    {
        public override string RegionName
        {
            get { return "LA1"; }
        }

        public override string InternalName
        {
            get { return "LA1"; }
        }

        public override string Locale
        {
            get { return "es_MX"; }
        }

        public override string ChatName
        {
            get { return "la1"; }
        }

        public override Uri NewsAddress
        {
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
                    //No known IP address
                };
            }
        }

        public override Uri SpectatorLink
        {
            get { return new Uri("http://spectator.la1.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "110.45.191.11:80"; }
        }
    }
}
