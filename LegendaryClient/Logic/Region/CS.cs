using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    /// <summary>
    ///     Allow users to specify custom servers (When servers change, this can also be used)
    /// </summary>
    internal class CS : BaseRegion
    {
        private static readonly Dictionary<string, string> vals = getSettings();
        private static string location;

        public override string Location
        {
            get { return Location; }
        }

        public override string Server
        {
            get { return vals["host"]; }
        }

        public override string RegionName
        {
            get { return vals["regionTag"]; }
        }

        public override bool Garena
        {
            get { return false; }
        }

        public override string LoginQueue
        {
            get { return vals["loginQueue"]; }
        }

        public override string InternalName
        {
            get { return vals["platformId"]; }
        }

        public override string ChatName
        {
            get { return vals["host"].Split('.')[1]; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/br/en_US.js"); } //Guess
        }

        public override string Locale
        {
            get { return "en_US"; } //Guess
        }

        public override IPAddress[] PingAddresses
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override Uri SpectatorLink
        {
            get { return new Uri(vals["featuredGamesURL"].Replace("featured", "")); }
        }

        public override string SpectatorIpAddress
        {
            get { return "nil"; } //Unknown, Get from the server
            set { }
        }

        private static Dictionary<string, string> getSettings()
        {
            var result = new Dictionary<string, string>();
            var file = new OpenFileDialog
            {
                Title = "Find League Of Legends",
                Multiselect = false
            };

            bool? showDialog = file.ShowDialog();
            if (showDialog != null && (bool) showDialog)
                result = file.FileName.LeagueSettingsReader();

            location = file.FileName;
            return result;
        }
    }
}