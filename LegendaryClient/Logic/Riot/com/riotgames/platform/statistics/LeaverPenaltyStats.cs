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
        public int Level { get; set; }

        [SerializedName("lastDecay")]
        public DateTime LastDecay { get; set; }

        [SerializedName("userInformed")]
        public bool UserInformed { get; set; }

        [SerializedName("points")]
        public int Points { get; set; }
    }
}
