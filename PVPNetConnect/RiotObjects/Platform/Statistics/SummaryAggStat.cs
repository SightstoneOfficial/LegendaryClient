using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class SummaryAggStat : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.SummaryAggStat";

        public SummaryAggStat(Callback callback)
        {
            this.callback = callback;
        }

        public SummaryAggStat(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummaryAggStat result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("statType")]
        public String StatType { get; set; }

        [InternalName("count")]
        public Double Count { get; set; }

        [InternalName("value")]
        public Double Value { get; set; }
    }
}