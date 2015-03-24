using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.spellbook.SpellBookDTO")]
    public class SpellBookDTO
    {
        [SerializedName("bookPagesJson")]
        public object BookPagesJson { get; set; }

        [SerializedName("bookPages")]
        public List<SpellBookPageDTO> BookPages { get; set; }

        [SerializedName("dateString")]
        public String DateString { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }

        [SerializedName("defaultPage")]
        public SpellBookPageDTO DefaultPage { get; set; }
    }
}
