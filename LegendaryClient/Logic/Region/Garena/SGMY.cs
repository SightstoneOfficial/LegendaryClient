using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.Region.Garena
{
    public sealed class SGMY : BaseRegion
    {
        public override string RegionName
        {
            get { return "prod.lol.garenanow.com"; }
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
            get { return "chat.lol.garenanow.com"; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/na/en_US.js"); } //http://lol.garena.com/landing.php?[garneaUser]
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
                return new IPAddress[]
                {
                    //No known IP address
                };
            }
        }

        public override Uri SpectatorLink
        {
            get { return new Uri("http://203.116.112.222:8088/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return ""; }
        }
    }
}
