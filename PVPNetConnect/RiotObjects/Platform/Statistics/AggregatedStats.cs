using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class AggregatedStats : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.AggregatedStats";

        public AggregatedStats()
        {
        }

        public AggregatedStats(Callback callback)
        {
            this.callback = callback;
        }

        public AggregatedStats(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(AggregatedStats result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("lifetimeStatistics")]
        public List<AggregatedStat> LifetimeStatistics { get; set; }

        [InternalName("modifyDate")]
        public object ModifyDate { get; set; }

        [InternalName("key")]
        public AggregatedStatsKey Key { get; set; }

        [InternalName("aggregatedStatsJson")]
        public String AggregatedStatsJson { get; set; }
    }
}