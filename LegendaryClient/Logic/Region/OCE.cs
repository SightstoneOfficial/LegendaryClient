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

        public override string Server
        {
            get { return "prod.oc1.lol.riotgames.com"; }
        }

        public override string LoginQueue
        {
            get { return "https://lq.oc1.lol.riotgames.com/"; }
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
            get { return "OC1"; }
        }

        public override string ChatName
        {
            get { return "oc1"; }
        }

        public override string Locale
        {
            get { return "en_US"; }
        }

        public override System.Uri NewsAddress
        {
            get { return new System.Uri("http://oce.leagueoflegends.com/en/rss.xml"); }
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
            get { return new System.Uri("http://spectator.oc1.lol.riotgames.com/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "192.64.169.29"; }
            set { }
        }
    }
}