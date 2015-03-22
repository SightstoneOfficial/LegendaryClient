using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.masterybook.TalentEntry")]
    public class TalentEntry
    {
        [SerializedName("rank")]
        public Int32 Rank { get; set; }

        [SerializedName("talentId")]
        public Int32 TalentId { get; set; }

        [SerializedName("talent")]
        public Talent Talent { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }
    }
}
