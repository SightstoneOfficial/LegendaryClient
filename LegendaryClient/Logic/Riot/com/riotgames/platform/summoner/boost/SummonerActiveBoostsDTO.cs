using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.boost.SummonerActiveBoostsDTO")]
    public class SummonerActiveBoostsDTO
    {
        [SerializedName("xpBoostEndDate")]
        public double XpBoostEndDate { get; set; }

        [SerializedName("xpBoostPerWinCount")]
        public int XpBoostPerWinCount { get; set; }

        [SerializedName("xpLoyaltyBoost")]
        public int XpLoyaltyBoost { get; set; }

        [SerializedName("ipBoostPerWinCount")]
        public int IpBoostPerWinCount { get; set; }

        [SerializedName("ipLoyaltyBoost")]
        public int IpLoyaltyBoost { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }

        [SerializedName("ipBoostEndDate")]
        public double IpBoostEndDate { get; set; }
    }
}
