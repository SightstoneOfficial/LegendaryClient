using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.catalog.ItemEffect")]
    public class ItemEffect
    {
        [SerializedName("effectId")]
        public int EffectId { get; set; }

        [SerializedName("itemEffectId")]
        public int ItemEffectId { get; set; }

        [SerializedName("effect")]
        public Effect Effect { get; set; }

        [SerializedName("value")]
        public String Value { get; set; }

        [SerializedName("itemId")]
        public int ItemId { get; set; }
    }
}
