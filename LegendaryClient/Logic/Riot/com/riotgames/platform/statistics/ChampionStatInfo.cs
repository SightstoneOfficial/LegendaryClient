using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.ChampionStatInfo")]
    public class ChampionStatInfo
    {
        [SerializedName("totalGamesPlayed")]
        public Int32 TotalGamesPlayed { get; set; }

        [SerializedName("accountId")]
        public Double AccountId { get; set; }

        [SerializedName("stats")]
        public List<AggregatedStat> Stats { get; set; }

        [SerializedName("championId")]
        public Double ChampionId { get; set; }
    }
}
