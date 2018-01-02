using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.ChampionStatInfo")]
    public class ChampionStatInfo
    {
        [SerializedName("totalGamesPlayed")]
        public int TotalGamesPlayed { get; set; }

        [SerializedName("accountId")]
        public double AccountId { get; set; }

        [SerializedName("stats")]
        public List<AggregatedStat> Stats { get; set; }

        [SerializedName("championId")]
        public double ChampionId { get; set; }
    }
}
