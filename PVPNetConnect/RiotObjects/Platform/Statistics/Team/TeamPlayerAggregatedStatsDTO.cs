using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics.Team
{
    public class TeamPlayerAggregatedStatsDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.team.TeamPlayerAggregatedStatsDTO";

        public TeamPlayerAggregatedStatsDTO()
        {
        }

        public TeamPlayerAggregatedStatsDTO(Callback callback)
        {
            this.callback = callback;
        }

        public TeamPlayerAggregatedStatsDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TeamPlayerAggregatedStatsDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("playerId")]
        public Double PlayerId { get; set; }

        [InternalName("aggregatedStats")]
        public AggregatedStats AggregatedStats { get; set; }
    }
}