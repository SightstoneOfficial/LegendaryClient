using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class BR : BaseRegion
    {
        public override string RegionName
        {
            get { return "BR"; }
        }

        public override string Server
        {
            get { return "prod.br.lol.riotgames.com"; }
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
            get { return "https://lq.br.lol.riotgames.com/"; }
        }

        public override string InternalName
        {
            get { return "BR1"; }
        }

        public override string ChatName
        {
            get { return "br"; }
        }

        public override System.Uri NewsAddress
        {
            get { return new System.Uri("http://br.leagueoflegends.com/pt/rss.xml"); }
            //This returns english (not spanish) characters
        }

        public override string Locale
        {
            get { return "en_US"; }
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
            get { return new System.Uri("http://spectator.br.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "66.151.33.19:80"; }
            set { }
        }
    }
}