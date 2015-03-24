using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.Talent")]
    public class Talent
    {
        [SerializedName("index")]
        public int Index { get; set; }

        [SerializedName("level5Desc")]
        public String Level5Desc { get; set; }

        [SerializedName("minLevel")]
        public int MinLevel { get; set; }

        [SerializedName("maxRank")]
        public int MaxRank { get; set; }

        [SerializedName("level4Desc")]
        public String Level4Desc { get; set; }

        [SerializedName("tltId")]
        public int TltId { get; set; }

        [SerializedName("level3Desc")]
        public String Level3Desc { get; set; }

        [SerializedName("talentGroupId")]
        public int TalentGroupId { get; set; }

        [SerializedName("gameCode")]
        public int GameCode { get; set; }

        [SerializedName("minTier")]
        public int MinTier { get; set; }

        [SerializedName("prereqTalentGameCode")]
        public object PrereqTalentGameCode { get; set; }

        [SerializedName("level2Desc")]
        public String Level2Desc { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("talentRowId")]
        public int TalentRowId { get; set; }

        [SerializedName("level1Desc")]
        public String Level1Desc { get; set; }
    }
}
