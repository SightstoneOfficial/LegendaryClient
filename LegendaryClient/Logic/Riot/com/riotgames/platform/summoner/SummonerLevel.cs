using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.SummonerLevel")]
    public class SummonerLevel
    {
        [SerializedName("expTierMod")]
        public Double ExpTierMod { get; set; }

        [SerializedName("grantRp")]
        public Double GrantRp { get; set; }

        [SerializedName("expForLoss")]
        public Double ExpForLoss { get; set; }

        [SerializedName("summonerTier")]
        public Double SummonerTier { get; set; }

        [SerializedName("infTierMod")]
        public Double InfTierMod { get; set; }

        [SerializedName("expToNextLevel")]
        public Double ExpToNextLevel { get; set; }

        [SerializedName("expForWin")]
        public Double ExpForWin { get; set; }

        [SerializedName("summonerLevel")]
        public Double Level { get; set; }
    }
}
