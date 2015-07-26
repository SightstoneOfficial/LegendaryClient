using System;
using System.Net;

namespace LegendaryClient.Logic.Region.Garena
{
    public sealed class VN : BaseRegion
    {
        public override string RegionName
        {
            get { return "VN"; }
        }

        public override string Server
        {
            get { return "prodvn1.lol.garenanow.com"; }
        }

        public override string LoginQueue
        {
            get { return "https://lqvn1.lol.garenanow.com"; }
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
            get { return "VN"; }
        }

        public override string ChatName
        {
            get { return "vn1"; }
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
            get { return new System.Uri("http://210.211.119.15:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "210.211.119.15:80"; }
            set { }
        }
    }
}
