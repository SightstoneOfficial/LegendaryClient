using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.AggregatedStatsKey")]
    public class AggregatedStatsKey
    {
        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("userId")]
        public double UserId { get; set; }

        [SerializedName("gameModeString")]
        public String GameModeString { get; set; }
    }
}
