#region

using System;
using System.Net;

#endregion

namespace LegendaryClient.Logic.Region
{
    public sealed class NA : BaseRegion
    {
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
            get { return "NA1"; }
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
                return new[]
                {
                    IPAddress.Parse("216.52.241.254"),
                    IPAddress.Parse("64.7.194.1"),
                    IPAddress.Parse("66.150.148.1")
                };
            }
        }

        public override string Location
        {
            get { return null; }
        }

        public override Uri SpectatorLink
        {
            get { return new Uri("http://spectator.na2.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "spectator.na2.lol.riotgames.com:80"; }
            set { }
        }

        public void NASqlite()
        {
            Client.sqlite = "gameStats_en_US.sqlite";
        }
    }
}