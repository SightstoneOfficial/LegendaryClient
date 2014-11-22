using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class NA : BaseRegion
    {
        public void NASqlite()
        {
            LegendaryClient.Logic.Client.sqlite = "gameStats_en_US.sqlite";
        }
        public override bool Garena
        {
            get { return false; }
        }

        public override string RegionName
        {
            get { return "NA"; }
        }

        public override string InternalName
        {
            get { return "NA2"; }
        }

        public override string ChatName
        {
            get { return "na2"; }
        }

        public override string Locale
        {
            get { return "en_US"; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/na/en_US.js"); }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.NA; }
        }

        public override IPAddress[] PingAddresses
        {
            get
            {
                return new IPAddress[]
                {
                    IPAddress.Parse("216.52.241.254"),
                    IPAddress.Parse("64.7.194.1"),
                    IPAddress.Parse("66.150.148.1")
                };
            }
        }

        public override Uri SpectatorLink
        {
            get { return new Uri("http://spectator.na1.lol.riotgames.com/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "216.133.234.17"; }
        }
    }
}