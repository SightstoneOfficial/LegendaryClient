using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.reroll.pojo.RollResult")]
    public class RollResult
    {
        [SerializedName("championId")]
        public int ChampionId { get; set; }

        [SerializedName("pointSummary")]
        public PointSummary PointSummary { get; set; }
    }
}
