using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.SummaryAggStats")]
    public class SummaryAggStats
    {
        [SerializedName("statsJson")]
        public object StatsJson { get; set; }

        [SerializedName("stats")]
        public List<SummaryAggStat> Stats { get; set; }
    }
}
