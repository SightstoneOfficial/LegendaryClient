﻿using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.PlayerStats")]
    public class PlayerStats
    {
        [SerializedName("timeTrackedStats")]
        public List<TimeTrackedStat> TimeTrackedStats { get; set; }

        [SerializedName("promoGamesPlayed")]
        public int PromoGamesPlayed { get; set; }

        [SerializedName("promoGamesPlayedLastUpdated")]
        public object PromoGamesPlayedLastUpdated { get; set; }
    }
}
