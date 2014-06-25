using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class PlayerStatSummaries : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.PlayerStatSummaries";

        public PlayerStatSummaries()
        {
        }

        public PlayerStatSummaries(Callback callback)
        {
            this.callback = callback;
        }

        public PlayerStatSummaries(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PlayerStatSummaries result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("playerStatSummarySet")]
        public List<PlayerStatSummary> PlayerStatSummarySet { get; set; }

        [InternalName("userId")]
        public Double UserId { get; set; }
    }
}