using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class PlayerStatSummary : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.PlayerStatSummary";

        public PlayerStatSummary()
        {
        }

        public PlayerStatSummary(Callback callback)
        {
            this.callback = callback;
        }

        public PlayerStatSummary(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PlayerStatSummary result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("maxRating")]
        public Int32 MaxRating { get; set; }

        [InternalName("playerStatSummaryTypeString")]
        public String PlayerStatSummaryTypeString { get; set; }

        [InternalName("aggregatedStats")]
        public SummaryAggStats AggregatedStats { get; set; }

        [InternalName("modifyDate")]
        public DateTime ModifyDate { get; set; }

        [InternalName("leaves")]
        public Int32 Leaves { get; set; }

        [InternalName("playerStatSummaryType")]
        public String PlayerStatSummaryType { get; set; }

        [InternalName("userId")]
        public Double UserId { get; set; }

        [InternalName("losses")]
        public Int32 Losses { get; set; }

        [InternalName("rating")]
        public Int32 Rating { get; set; }

        [InternalName("aggregatedStatsJson")]
        public object AggregatedStatsJson { get; set; }

        [InternalName("wins")]
        public Int32 Wins { get; set; }
    }
}