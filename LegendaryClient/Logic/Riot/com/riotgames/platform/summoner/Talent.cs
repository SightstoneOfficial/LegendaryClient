using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.Talent")]
    public class Talent
    {
        [SerializedName("index")]
        public Int32 Index { get; set; }

        [SerializedName("level5Desc")]
        public String Level5Desc { get; set; }

        [SerializedName("minLevel")]
        public Int32 MinLevel { get; set; }

        [SerializedName("maxRank")]
        public Int32 MaxRank { get; set; }

        [SerializedName("level4Desc")]
        public String Level4Desc { get; set; }

        [SerializedName("tltId")]
        public Int32 TltId { get; set; }

        [SerializedName("level3Desc")]
        public String Level3Desc { get; set; }

        [SerializedName("talentGroupId")]
        public Int32 TalentGroupId { get; set; }

        [SerializedName("gameCode")]
        public Int32 GameCode { get; set; }

        [SerializedName("minTier")]
        public Int32 MinTier { get; set; }

        [SerializedName("prereqTalentGameCode")]
        public object PrereqTalentGameCode { get; set; }

        [SerializedName("level2Desc")]
        public String Level2Desc { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("talentRowId")]
        public Int32 TalentRowId { get; set; }

        [SerializedName("level1Desc")]
        public String Level1Desc { get; set; }
    }
}
