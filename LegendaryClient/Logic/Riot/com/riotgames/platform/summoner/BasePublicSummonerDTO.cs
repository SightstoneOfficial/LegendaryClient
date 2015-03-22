using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.BasePublicSummonerDTO")]
    public class BasePublicSummonerDTO
    {
        [SerializedName("seasonTwoTier")]
        public String SeasonTwoTier { get; set; }

        [SerializedName("publicName")]
        public String InternalName { get; set; }

        [SerializedName("seasonOneTier")]
        public String SeasonOneTier { get; set; }

        [SerializedName("acctId")]
        public Double AcctId { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("sumId")]
        public Double SumId { get; set; }

        [SerializedName("profileIconId")]
        public Int32 ProfileIconId { get; set; }
    }
}
