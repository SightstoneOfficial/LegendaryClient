#region

using System;
using System.Net;

#endregion

namespace LegendaryClient.Logic.Region.Garena
{
    public sealed class ID : BaseRegion
    {
        public override string Location
        {
            get { return null; }
        }

        public override string RegionName
        {
            get { return "ID"; }
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

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.ID; }
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
