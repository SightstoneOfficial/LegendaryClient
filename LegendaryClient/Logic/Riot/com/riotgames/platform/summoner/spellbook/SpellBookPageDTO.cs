using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.spellbook.SpellBookPageDTO")]
    public class SpellBookPageDTO
    {
        [SerializedName("slotEntries")]
        public List<SlotEntry> SlotEntries { get; set; }

        [SerializedName("summonerId")]
        public Int32 SummonerId { get; set; }

        [SerializedName("createDate")]
        public DateTime CreateDate { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("pageId")]
        public Int32 PageId { get; set; }

        [SerializedName("current")]
        public Boolean Current { get; set; }
    }
}
