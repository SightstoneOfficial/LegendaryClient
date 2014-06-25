using PVPNetConnect.RiotObjects.Team;
using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Statistics.Team
{
    public class TeamAggregatedStatsDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.team.TeamAggregatedStatsDTO";

        public TeamAggregatedStatsDTO()
        {
        }

        public TeamAggregatedStatsDTO(Callback callback)
        {
            this.callback = callback;
        }

        public TeamAggregatedStatsDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TeamAggregatedStatsDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("queueType")]
        public String QueueType { get; set; }

        [InternalName("serializedToJson")]
        public String SerializedToJson { get; set; }

        [InternalName("playerAggregatedStatsList")]
        public List<TeamPlayerAggregatedStatsDTO> PlayerAggregatedStatsList { get; set; }

        [InternalName("teamId")]
        public TeamId TeamId { get; set; }
    }
}