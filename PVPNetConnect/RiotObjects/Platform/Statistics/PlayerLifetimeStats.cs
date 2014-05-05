using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class PlayerLifetimeStats : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.PlayerLifetimeStats";

        public PlayerLifetimeStats()
        {
        }

        public PlayerLifetimeStats(Callback callback)
        {
            this.callback = callback;
        }

        public PlayerLifetimeStats(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PlayerLifetimeStats result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("playerStatSummaries")]
        public PlayerStatSummaries PlayerStatSummaries { get; set; }

        [InternalName("leaverPenaltyStats")]
        public LeaverPenaltyStats LeaverPenaltyStats { get; set; }

        [InternalName("previousFirstWinOfDay")]
        public DateTime PreviousFirstWinOfDay { get; set; }

        [InternalName("userId")]
        public Double UserId { get; set; }

        [InternalName("dodgeStreak")]
        public Int32 DodgeStreak { get; set; }

        [InternalName("dodgePenaltyDate")]
        public object DodgePenaltyDate { get; set; }

        [InternalName("playerStatsJson")]
        public object PlayerStatsJson { get; set; }

        [InternalName("playerStats")]
        public PlayerStats PlayerStats { get; set; }
    }
}