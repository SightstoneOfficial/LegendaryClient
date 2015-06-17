using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class LAN : BaseRegion
    {
        public override string RegionName
        {
            get { return "LAN"; }
        }

        public override string Server
        {
            get { return "prod.la1.lol.riotgames.com"; }
        }

        public override string Location
        {
            get { return null; }
        }

        public override bool Garena
        {
            get { return false; }
        }

        public override string LoginQueue
        {
            get { return "https://lq.la1.lol.riotgames.com/"; }
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

        public override System.Uri NewsAddress
        {
            get { return new System.Uri("http://lan.leagueoflegends.com/es/rss.xml"); }
        }

        public override IPAddress[] PingAddresses
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override System.Uri SpectatorLink
        {
            get { return new System.Uri("http://spectator.la1.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "110.45.191.11:80"; }
            set { }
        }
    }
}
