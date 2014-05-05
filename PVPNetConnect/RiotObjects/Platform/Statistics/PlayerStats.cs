using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class PlayerStats : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.PlayerStats";

        public PlayerStats()
        {
        }

        public PlayerStats(Callback callback)
        {
            this.callback = callback;
        }

        public PlayerStats(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PlayerStats result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("timeTrackedStats")]
        public List<TimeTrackedStat> TimeTrackedStats { get; set; }

        [InternalName("promoGamesPlayed")]
        public Int32 PromoGamesPlayed { get; set; }

        [InternalName("promoGamesPlayedLastUpdated")]
        public object PromoGamesPlayedLastUpdated { get; set; }
    }
}