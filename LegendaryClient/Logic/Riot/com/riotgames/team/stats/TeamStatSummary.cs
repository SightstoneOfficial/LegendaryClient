using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Team
{
    [Serializable]
    [SerializedName("com.riotgames.team.stats.TeamStatSummary")]
    public class TeamStatSummary
    {
        [SerializedName("teamStatDetails")]
        public List<TeamStatDetail> TeamStatDetails { get; set; }

        [SerializedName("teamIdString")]
        public String TeamIdString { get; set; }

        [SerializedName("teamId")]
        public TeamId TeamId { get; set; }
    }
}
