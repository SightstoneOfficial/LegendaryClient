using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class KR : BaseRegion
    {
        public void NASqlite()
        {
            LegendaryClient.Logic.Client.sqlite = "gameStats_ko_KR.sqlite";
        }
        public override string Location
        {
            get { return null; }
        }

        public override string RegionName
        {
            get { return "KR"; }
        }

        public override bool Garena
        {
            get { return false; }
        }

        public override string InternalName
        {
            get { return "KR"; }
        }

        public override string Locale
        {
            get { return "ko_KR"; }
        }

        public override string ChatName
        {
            get { return "kr"; }
        }

        //No known news URI for Korea
        public override Uri NewsAddress
        {
            get { return new Uri(""); }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.KR; }
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
            get { return new Uri("http://spectator.kr.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "110.45.191.11:80"; }
            set {  }
        }
    }
}