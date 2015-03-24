using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.AggregatedStat")]
    public class AggregatedStat
    {
        [SerializedName("statType")]
        public String StatType { get; set; }

        [SerializedName("count")]
        public double Count { get; set; }

        [SerializedName("value")]
        public double Value { get; set; }

        [SerializedName("championId")]
        public double ChampionId { get; set; }
    }
}
