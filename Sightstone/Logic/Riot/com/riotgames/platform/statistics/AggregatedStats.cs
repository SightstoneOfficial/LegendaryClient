using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.AggregatedStats")]
    public class AggregatedStats
    {
        [SerializedName("lifetimeStatistics")]
        public List<AggregatedStat> LifetimeStatistics { get; set; }

        [SerializedName("modifyDate")]
        public object ModifyDate { get; set; }

        [SerializedName("key")]
        public AggregatedStatsKey Key { get; set; }

        [SerializedName("aggregatedStatsJson")]
        public String AggregatedStatsJson { get; set; }
    }
}
