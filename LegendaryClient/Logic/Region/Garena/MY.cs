using System;
using System.Net;

namespace LegendaryClient.Logic.Region.Garena
{
    public sealed class MY : BaseRegion
    {
        public override string Location
        {
            get { return null; }
        }

        public override string Server
        {
            get { return "prod.lol.garenanow.com"; }
        }

        public override string LoginQueue
        {
            get { return "https://lq.lol.garenanow.com/"; }
        }

        public override string RegionName
        {
            get { return "MY"; }
        }

        public override bool Garena
        {
            get { return true; }
        }

        public override string InternalName
        {
            get { return "MY"; }
        }

        public override string ChatName
        {
            get { return ""; }
        }

        public override System.Uri NewsAddress
        {
            get { return new System.Uri("http://ll.leagueoflegends.com/landingpage/data/na/en_US.js"); }
            //http://lol.garena.com/landing.php?[garneaUser]
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
            get { return new System.Uri("http://203.116.112.222:8088/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "203.116.112.222:8088"; }
            set { }
        }
    }
}
