using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.PlayerLifetimeStats")]
    public class PlayerLifetimeStats
    {
        [SerializedName("playerStatSummaries")]
        public PlayerStatSummaries PlayerStatSummaries { get; set; }

        [SerializedName("leaverPenaltyStats")]
        public LeaverPenaltyStats LeaverPenaltyStats { get; set; }

        [SerializedName("previousFirstWinOfDay")]
        public DateTime PreviousFirstWinOfDay { get; set; }

        [SerializedName("userId")]
        public Double UserId { get; set; }

        [SerializedName("dodgeStreak")]
        public Int32 DodgeStreak { get; set; }

        [SerializedName("dodgePenaltyDate")]
        public object DodgePenaltyDate { get; set; }

        [SerializedName("playerStatsJson")]
        public object PlayerStatsJson { get; set; }

        [SerializedName("playerStats")]
        public PlayerStats PlayerStats { get; set; }
    }
}
