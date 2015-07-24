using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.catalog.runes.Rune")]
    public class Rune
    {
        [SerializedName("imagePath")]
        public object ImagePath { get; set; }

        [SerializedName("toolTip")]
        public object ToolTip { get; set; }

        [SerializedName("tier")]
        public int Tier { get; set; }

        [SerializedName("itemId")]
        public int ItemId { get; set; }

        [SerializedName("runeType")]
        public RuneType RuneType { get; set; }

        [SerializedName("duration")]
        public int Duration { get; set; }

        [SerializedName("gameCode")]
        public int GameCode { get; set; }

        [SerializedName("itemEffects")]
        public List<ItemEffect> ItemEffects { get; set; }

        [SerializedName("baseType")]
        public String BaseType { get; set; }

        [SerializedName("description")]
        public String Description { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("uses")]
        public object Uses { get; set; }
    }
}
