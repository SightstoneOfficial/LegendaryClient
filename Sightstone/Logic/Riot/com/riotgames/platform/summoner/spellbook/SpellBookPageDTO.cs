﻿using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.spellbook.SpellBookPageDTO")]
    public class SpellBookPageDTO
    {
        [SerializedName("slotEntries")]
        public List<SlotEntry> SlotEntries { get; set; }

        [SerializedName("summonerId")]
        public int SummonerId { get; set; }

        [SerializedName("createDate")]
        public DateTime CreateDate { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("pageId")]
        public int PageId { get; set; }

        [SerializedName("current")]
        public bool Current { get; set; }
    }
}
