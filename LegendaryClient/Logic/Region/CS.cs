using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.Region
{
    /// <summary>
    /// Allow users to specify custom servers (When servers change, this can also be used)
    /// </summary>
    class CS : BaseRegion
    {
        private static Dictionary<String, String> vals = getSettings();

        private static Dictionary<String, String> getSettings()
        {
            Dictionary<String, String> result = new Dictionary<String, String>();
            OpenFileDialog file = new OpenFileDialog();
            file.Title = "Find League Of Legends";
            file.Multiselect = false;

            if ((bool)file.ShowDialog())
            {
                file.FileName = "";
                    
            }

            return result;
        }

        public override string RegionName
        {
            get { return "BR"; }
        }

        public override bool Garena
        {
            get { return false; }
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

        public override string Locale
        {
            get { return "en_US"; }
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
            get { return "66.151.33.19:80"; }
        }
    }
}
