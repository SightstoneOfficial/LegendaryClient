#region

using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.Win32;

#endregion

namespace LegendaryClient.Logic.Region
{
    /// <summary>
    ///     Allow users to specify custom servers (When servers change, this can also be used)
    /// </summary>
    internal class CS : BaseRegion
    {
        //private static Dictionary<String, String> _vals = getSettings();

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
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/br/en_US.js"); }
            //This returns english (not spanish) characters
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

        private static Dictionary<String, String> GetSettings()
        {
            var result = new Dictionary<String, String>();
            var file = new OpenFileDialog
            {
                Title = "Find League Of Legends",
                Multiselect = false
            };

            bool? showDialog = file.ShowDialog();
            if (showDialog != null && (bool) showDialog)
                file.FileName = "";

            return result;
        }
    }
}