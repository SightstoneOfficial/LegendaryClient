using System;
using System.Net;

namespace Sightstone.Logic.Region.Garena
{
    public sealed class TH : BaseRegion
    {
        public override string RegionName
        {
            get { return "th"; }
        }

        public override string Server
        {
            get { return "prodth.lol.garenanow.com"; }
        }

        public override string LoginQueue
        {
            get { return "https://lqth.lol.garenanow.com/"; }
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
            get { return "TH"; }
        }

        public override string ChatName
        {
            get { return "th"; }
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
                return new[]
                {
                    IPAddress.Parse("112.121.158.28"),
                    IPAddress.Parse("112.121.157.15")
                };
            }
        }

        public override System.Uri SpectatorLink
        {
            get { return new System.Uri("http://112.121.157.15:8088/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "112.121.157.15:8088"; }
            set { }
        }
    }
}
