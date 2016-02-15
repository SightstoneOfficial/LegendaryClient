using System;
using RtmpSharp.IO;
using System.Collections.Generic;
using Sightstone.Logic.Riot.Team;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.team.TeamAggregatedStatsDTO")]
    public class TeamAggregatedStatsDTO
    {
        [SerializedName("queueType")]
        public String QueueType { get; set; }

        [SerializedName("serializedToJson")]
        public String SerializedToJson { get; set; }

        [SerializedName("playerAggregatedStatsList")]
        public List<TeamPlayerAggregatedStatsDTO> PlayerAggregatedStatsList { get; set; }

        [SerializedName("teamId")]
        public TeamId TeamId { get; set; }
    }
}
