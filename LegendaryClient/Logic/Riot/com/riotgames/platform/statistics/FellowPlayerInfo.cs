using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.FellowPlayerInfo")]
    public class FellowPlayerInfo
    {
        [SerializedName("championId")]
        public double ChampionId { get; set; }

        [SerializedName("teamId")]
        public int TeamId { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }
    }
}
