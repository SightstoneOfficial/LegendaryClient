using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.reroll.pojo.PointSummary")]
    public class PointSummary
    {
        [SerializedName("pointsToNextRoll")]
        public Double PointsToNextRoll { get; set; }

        [SerializedName("maxRolls")]
        public Int32 MaxRolls { get; set; }

        [SerializedName("numberOfRolls")]
        public Int32 NumberOfRolls { get; set; }

        [SerializedName("pointsCostToRoll")]
        public Double PointsCostToRoll { get; set; }

        [SerializedName("currentPoints")]
        public Double CurrentPoints { get; set; }
    }
}
