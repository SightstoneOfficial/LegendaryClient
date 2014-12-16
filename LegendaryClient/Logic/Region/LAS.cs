#region

using System;
using System.Net;

#endregion

namespace LegendaryClient.Logic.Region
{
    public sealed class LAS : BaseRegion
    {
        public override string RegionName
        {
            get { return "LAS"; }
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

        public override Uri NewsAddress
        {
            get { return new Uri("http://las.leagueoflegends.com/es/rss.xml"); }
        }

        public override bool Garena
        {
            get { return false; }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.LA2; }
        }

        public override string Locale
        {
            get { return "en_US"; }
        }

        public override IPAddress[] PingAddresses
        {
            get
            {
                return new IPAddress[]
                {
                    //No known IP address
                };
            }
        }

        public override Uri SpectatorLink
        {
            get { return new Uri("http://spectator.la2.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "66.151.33.19:80"; }
            set { }
        }
    }
}