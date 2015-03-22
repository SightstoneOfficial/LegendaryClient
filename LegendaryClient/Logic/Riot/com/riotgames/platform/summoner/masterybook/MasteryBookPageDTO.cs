using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.masterybook.MasteryBookPageDTO")]
    public class MasteryBookPageDTO
    {
        [SerializedName("talentEntries")]
        public List<TalentEntry> TalentEntries { get; set; }

        [SerializedName("pageId")]
        public Double PageId { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("current")]
        public Boolean Current { get; set; }

        [SerializedName("createDate")]
        public object CreateDate { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }
    }
}
