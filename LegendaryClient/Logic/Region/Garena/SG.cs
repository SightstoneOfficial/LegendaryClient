#region

using System;
using System.Net;

#endregion

namespace LegendaryClient.Logic.Region
{
    public sealed class SG : BaseRegion
    {
        public override string RegionName
        {
            get { return "SG"; }
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
            get { return "SG"; }
        }

        public override string ChatName
        {
            get { return ""; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/na/en_US.js"); }
            //http://lol.garena.com/landing.php?[garneaUser]
        }

        public override string Locale
        {
            get { return "en_SG"; }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.SG; }
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
            get { return new Uri("http://203.116.112.222:8088/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "203.116.112.222:8088"; }
            set { }
        }
    }
}
