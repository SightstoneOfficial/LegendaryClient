using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.boost.SummonerActiveBoostsDTO")]
    public class SummonerActiveBoostsDTO
    {
        [SerializedName("xpBoostEndDate")]
        public Double XpBoostEndDate { get; set; }

        [SerializedName("xpBoostPerWinCount")]
        public Int32 XpBoostPerWinCount { get; set; }

        [SerializedName("xpLoyaltyBoost")]
        public Int32 XpLoyaltyBoost { get; set; }

        [SerializedName("ipBoostPerWinCount")]
        public Int32 IpBoostPerWinCount { get; set; }

        [SerializedName("ipLoyaltyBoost")]
        public Int32 IpLoyaltyBoost { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }

        [SerializedName("ipBoostEndDate")]
        public Double IpBoostEndDate { get; set; }
    }
}
