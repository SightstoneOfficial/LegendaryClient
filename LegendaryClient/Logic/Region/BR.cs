using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class BR : BaseRegion
    {
        public override string RegionName
        {
            get { return "BR"; }
        }

        public override string InternalName
        {
            get { return "BR1"; }
        }

        public override string ChatName
        {
            get { return "br"; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/br/en_US.js"); } //This returns english (not spanish) characters
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.BR; }
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
            get { return new Uri("http://spectator.br.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "66.150.148.234"; }
        }
    }
}