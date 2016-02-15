﻿using System;
using System.Net;

namespace LegendaryClient.Logic.Region
{
    public sealed class EUN : BaseRegion
    {
        public override string RegionName
        {
            get { return "EUN"; }
        }

        public override string InternalName
        {
            get { return "EUN1"; }
        }

        public override string ChatName
        {
            get { return "eun1"; }
        }

        public override Uri NewsAddress
        {
            get { return new Uri("http://ll.leagueoflegends.com/landingpage/data/eune/en_GB.js"); }
        }

        public override string Server
        {
            get { return "prod.eun1.lol.riotgames.com"; }
        }

        public override string LoginQueue
        {
            get { return "https://lq.eun1.lol.riotgames.com/"; }
        }

        public override string Locale
        {
            get { return "en_GB"; }
        }

        public override IPAddress[] PingAddresses
        {
            get
            {
                return new IPAddress[]
                {
                    IPAddress.Parse("66.150.148.1"),
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
            get { return "95.172.65.26"; }
        }
    }
}