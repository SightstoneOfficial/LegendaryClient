using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class AggregatedStat : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.AggregatedStat";

        public AggregatedStat()
        {
        }

        public AggregatedStat(Callback callback)
        {
            this.callback = callback;
        }

        public AggregatedStat(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(AggregatedStat result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("statType")]
        public string StatType { get; set; }

        [InternalName("count")]
        public double Count { get; set; }

        [InternalName("value")]
        public double Value { get; set; }

        [InternalName("championId")]
        public double ChampionId { get; set; }
    }
}