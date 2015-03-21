using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class AggregatedStatsKey : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.AggregatedStatsKey";

        public AggregatedStatsKey()
        {
        }

        public AggregatedStatsKey(Callback callback)
        {
            this.callback = callback;
        }

        public AggregatedStatsKey(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(AggregatedStatsKey result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("gameMode")]
        public string GameMode { get; set; }

        [InternalName("userId")]
        public double UserId { get; set; }

        [InternalName("gameModeString")]
        public string GameModeString { get; set; }
    }
}