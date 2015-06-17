using System;
using System.Net;

namespace LegendaryClient.Logic.Region.Garena
{
    public sealed class TW : BaseRegion
    {
        public override string RegionName
        {
            get { return "tw"; }
        }

        public override string Server
        {
            get { return "prodtw.lol.garenanow.com"; }
        }

        public override string LoginQueue
        {
            get { return "https://loginqueuetw.lol.garenanow.com/"; }
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
            get { return "TW"; }
        }

        public override string ChatName
        {
            get { return "tw"; }
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
            get { return new System.Uri("http://112.121.84.194:8088/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return ""; }
            set { }
        }
    }
}
