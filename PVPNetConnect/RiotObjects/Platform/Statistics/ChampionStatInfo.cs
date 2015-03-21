using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class ChampionStatInfo : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.ChampionStatInfo";

        public ChampionStatInfo()
        {
        }

        public ChampionStatInfo(Callback callback)
        {
            this.callback = callback;
        }

        public ChampionStatInfo(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(ChampionStatInfo result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("totalGamesPlayed")]
        public int TotalGamesPlayed { get; set; }

        [InternalName("accountId")]
        public double AccountId { get; set; }

        [InternalName("stats")]
        public List<AggregatedStat> Stats { get; set; }

        [InternalName("championId")]
        public double ChampionId { get; set; }
    }
}