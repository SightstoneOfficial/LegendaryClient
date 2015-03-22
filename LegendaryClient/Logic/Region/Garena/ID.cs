using System;
using System.Net;

namespace LegendaryClient.Logic.Region.Garena
{
    public sealed class ID : BaseRegion
    {
        public override string RegionName
        {
            get { return "id"; }
        }

        public override string Server
        {
            get { return "prodid.lol.garenanow.com"; }
        }

        public override string LoginQueue
        {
            get { return "https://lqid.lol.garenanow.com/"; }
        }

        public override string Location
        {
            get { return null; }
        }

        public override bool Garena
        {
            get { return true; }
        }

        public override string InternalName
        {
            get { return "ID"; }
        }

        public override string ChatName
        {
            get { return "id"; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/na/en_US.js"); }
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

        public override Uri SpectatorLink
        {
            get { return new Uri("http://103.248.58.26:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "103.248.58.26:80"; }
            set { }
        }
    }
}
