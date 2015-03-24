using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.masterybook.TalentEntry")]
    public class TalentEntry
    {
        [SerializedName("rank")]
        public int Rank { get; set; }

        [SerializedName("talentId")]
        public int TalentId { get; set; }

        [SerializedName("talent")]
        public Talent Talent { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }
    }
}
