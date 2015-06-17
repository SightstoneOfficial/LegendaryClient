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

        public override string Server
        {
            get { return "prod.kr.lol.riotgames.com"; }
        }

        public override string RegionName
        {
            get { return "KR"; }
        }

        public override bool Garena
        {
            get { return false; }
        }

        public override string LoginQueue
        {
            get { return "https://lq.kr.lol.riotgames.com/"; }
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

        //No known news System.Uri for Korea
        public override System.Uri NewsAddress
        {
            get { return new System.Uri(""); }
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
            get { return new System.Uri("http://spectator.kr.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "110.45.191.11:80"; }
            set {  }
        }
    }
}