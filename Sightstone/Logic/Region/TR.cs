﻿using System;
using System.Net;

namespace Sightstone.Logic.Region
{
    public sealed class TR : BaseRegion
    {
        public override string RegionName
        {
            get { return "TR"; }
        }

        public override string Server
        {
            get { return "prod.tr.lol.riotgames.com"; }
        }

        public override string LoginQueue
        {
            get { return "https://lq.tr.lol.riotgames.com/"; }
        }

        public override string Location
        {
            get { return null; }
        }

        public override bool Garena
        {
            get { return false; }
        }

        public override string InternalName
        {
            get { return "TR1"; }
        }

        public override string ChatName
        {
            get { return "tr"; }
        }

        public override string Locale
        {
            get { return "en_US"; }
        }

        public override System.Uri NewsAddress
        {
            get { return new System.Uri("http://tr.leagueoflegends.com/tr/rss.xml"); }
            //This returns english (not spanish) characters
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
            get { return new System.Uri("http://spectator.tr.lol.riotgames.com:80/observer-mode/rest/"); }
        }

        public override string SpectatorIpAddress
        {
            get { return "95.172.65.242:80"; }
            set { }
        }
    }
}