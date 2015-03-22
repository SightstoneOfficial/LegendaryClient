using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.reroll.pojo.EogPointChangeBreakdown")]
    public class EogPointChangeBreakdown
    {
        [SerializedName("pointChangeFromGamePlay")]
        public Double PointChangeFromGamePlay { get; set; }

        [SerializedName("pointChangeFromChampionsOwned")]
        public Double PointChangeFromChampionsOwned { get; set; }

        [SerializedName("previousPoints")]
        public Double PreviousPoints { get; set; }

        [SerializedName("pointsUsed")]
        public Double PointsUsed { get; set; }

        [SerializedName("endPoints")]
        public Double EndPoints { get; set; }
    }
}
