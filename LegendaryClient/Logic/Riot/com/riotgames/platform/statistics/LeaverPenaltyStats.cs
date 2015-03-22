using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.LeaverPenaltyStats")]
    public class LeaverPenaltyStats
    {
        [SerializedName("lastLevelIncrease")]
        public object LastLevelIncrease { get; set; }

        [SerializedName("level")]
        public Int32 Level { get; set; }

        [SerializedName("lastDecay")]
        public DateTime LastDecay { get; set; }

        [SerializedName("userInformed")]
        public Boolean UserInformed { get; set; }

        [SerializedName("points")]
        public Int32 Points { get; set; }
    }
}
