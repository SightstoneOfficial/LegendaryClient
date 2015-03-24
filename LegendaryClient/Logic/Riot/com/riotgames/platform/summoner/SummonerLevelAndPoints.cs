using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.SummonerLevelAndPoints")]
    public class SummonerLevelAndPoints
    {
        [SerializedName("infPoints")]
        public double InfPoints { get; set; }

        [SerializedName("expPoints")]
        public double ExpPoints { get; set; }

        [SerializedName("summonerLevel")]
        public double SummonerLevel { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }
    }
}
