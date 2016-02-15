using System;
using System.Net;

namespace Sightstone.Logic.Region
{
    public sealed class LAS : BaseRegion
    {
        public override string RegionName
        {
            get { return "LAS"; }
        }

        public override string Server
        {
            get { return "prod.la2.lol.riotgames.com"; }
        }

        public override string Location
        {
            get { return null; }
        }

        public override string InternalName
        {
            get { return "LA2"; }
        }

        public override string ChatName
        {
            get { return "la2"; }
        }

        public override System.Uri NewsAddress
        {
            get { return new System.Uri("http://las.leagueoflegends.com/es/rss.xml"); }
        }

        public override bool Garena
        {
            get { return false; }
        }

        public override string LoginQueue
        {
            get { return "https://lq.la2.lol.riotgames.com/"; }
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
            get { return new System.Uri("http://spectator.la2.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "66.151.33.19:80"; }
            set { }
        }
    }
}