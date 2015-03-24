using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.reroll.pojo.EogPointChangeBreakdown")]
    public class EogPointChangeBreakdown
    {
        [SerializedName("pointChangeFromGamePlay")]
        public double PointChangeFromGamePlay { get; set; }

        [SerializedName("pointChangeFromChampionsOwned")]
        public double PointChangeFromChampionsOwned { get; set; }

        [SerializedName("previousPoints")]
        public double PreviousPoints { get; set; }

        [SerializedName("pointsUsed")]
        public double PointsUsed { get; set; }

        [SerializedName("endPoints")]
        public double EndPoints { get; set; }
    }
}
