using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class RU : BaseRegion
    {
        public override string RegionName
        {
            get { return "RU"; }
        }

        public override string InternalName
        {
            get { return "RU"; }
        }

        public override string ChatName
        {
            get { return "ru"; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/ru/ru_RU.js"); }
        }

        public override PVPNetConnect.Region PVPRegion
        {
            get { return PVPNetConnect.Region.RU; }
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
            get { return new Uri("http://spectator.ru.lol.riotgames.com/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "95.172.65.242"; }
        }
    }
}