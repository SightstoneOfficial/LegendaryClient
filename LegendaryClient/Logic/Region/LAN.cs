#region

using System;
using System.Net;

#endregion

namespace LegendaryClient.Logic.Region
{
    public sealed class LAN : BaseRegion
    {
        public override string RegionName
        {
            get { return "LAN"; }
        }

        public override string Location
        {
            get { return null; }
        }

        public override bool Garena
        {
            get { return false; }
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
            get { return new Uri("http://lan.leagueoflegends.com/es/rss.xml"); }
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
            set { }
        }
    }
}
