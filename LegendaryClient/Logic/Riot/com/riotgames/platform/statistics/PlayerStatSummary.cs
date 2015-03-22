using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.PlayerStatSummary")]
    public class PlayerStatSummary
    {
        [SerializedName("maxRating")]
        public Int32 MaxRating { get; set; }

        [SerializedName("playerStatSummaryTypeString")]
        public String PlayerStatSummaryTypeString { get; set; }

        [SerializedName("aggregatedStats")]
        public SummaryAggStats AggregatedStats { get; set; }

        [SerializedName("modifyDate")]
        public DateTime ModifyDate { get; set; }

        [SerializedName("leaves")]
        public object Leaves { get; set; }

        [SerializedName("playerStatSummaryType")]
        public String PlayerStatSummaryType { get; set; }

        [SerializedName("userId")]
        public Double UserId { get; set; }

        [SerializedName("losses")]
        public Int32 Losses { get; set; }

        [SerializedName("rating")]
        public Int32 Rating { get; set; }

        [SerializedName("aggregatedStatsJson")]
        public object AggregatedStatsJson { get; set; }

        [SerializedName("wins")]
        public Int32 Wins { get; set; }
    }
}
