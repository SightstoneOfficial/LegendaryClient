using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.SummonerLevelAndPoints")]
    public class SummonerLevelAndPoints
    {
        [SerializedName("infPoints")]
        public Double InfPoints { get; set; }

        [SerializedName("expPoints")]
        public Double ExpPoints { get; set; }

        [SerializedName("summonerLevel")]
        public Double SummonerLevel { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }
    }
}
