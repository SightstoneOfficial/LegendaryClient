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
        public int MaxRating { get; set; }

        [InternalName("playerStatSummaryTypeString")]
        public string PlayerStatSummaryTypeString { get; set; }

        [InternalName("aggregatedStats")]
        public SummaryAggStats AggregatedStats { get; set; }

        [InternalName("modifyDate")]
        public DateTime ModifyDate { get; set; }

        [InternalName("leaves")]
        public int Leaves { get; set; }

        [InternalName("playerStatSummaryType")]
        public string PlayerStatSummaryType { get; set; }

        [InternalName("userId")]
        public double UserId { get; set; }

        [InternalName("losses")]
        public int Losses { get; set; }

        [InternalName("rating")]
        public int Rating { get; set; }

        [InternalName("aggregatedStatsJson")]
        public object AggregatedStatsJson { get; set; }

        [InternalName("wins")]
        public int Wins { get; set; }
    }
}