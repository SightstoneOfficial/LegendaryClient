using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.team.TeamPlayerAggregatedStatsDTO")]
    public class TeamPlayerAggregatedStatsDTO
    {
        [SerializedName("playerId")]
        public double PlayerId { get; set; }

        [SerializedName("aggregatedStats")]
        public AggregatedStats AggregatedStats { get; set; }
    }
}
