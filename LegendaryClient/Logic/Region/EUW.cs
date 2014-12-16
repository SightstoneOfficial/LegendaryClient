#region

using System;
using System.Net;

#endregion

namespace LegendaryClient.Logic.Region
{
    public sealed class EUW : BaseRegion
    {
        public override string RegionName
        {
            get { return "EUW"; }
        }

        public override bool Garena
        {
            get { return false; }
        }

        public override string Location
        {
            get { return null; }
        }

        public override string InternalName
        {
            get { return "EUW1"; }
        }

        public override string ChatName
        {
            get { return "euw1"; }
        }

        public override string Locale
        {
            get { return "en_GB"; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://euw.leagueoflegends.com/en/rss.xml"); }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.EUW; }
        }

        public override IPAddress[] PingAddresses
        {
            get
            {
                return new[]
                {
                    IPAddress.Parse("64.7.194.1"),
                    IPAddress.Parse("95.172.65.1") //This one seems to give high ping
                };
            }
        }

        public override Uri SpectatorLink
        {
            get { return new Uri("http://spectator.eu.lol.riotgames.com:8088/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "95.172.65.26:8088"; }
            set { }
        }
    }
}