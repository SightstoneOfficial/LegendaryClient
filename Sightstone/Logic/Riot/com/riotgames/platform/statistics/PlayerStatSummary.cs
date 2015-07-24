using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.PlayerStatSummary")]
    public class PlayerStatSummary
    {
        [SerializedName("maxRating")]
        public int MaxRating { get; set; }

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
        public double UserId { get; set; }

        [SerializedName("losses")]
        public int Losses { get; set; }

        [SerializedName("rating")]
        public int Rating { get; set; }

        [SerializedName("aggregatedStatsJson")]
        public object AggregatedStatsJson { get; set; }

        [SerializedName("wins")]
        public int Wins { get; set; }
    }
}
