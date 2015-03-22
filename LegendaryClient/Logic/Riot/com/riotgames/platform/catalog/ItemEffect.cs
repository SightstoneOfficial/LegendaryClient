using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.catalog.ItemEffect")]
    public class ItemEffect
    {
        [SerializedName("effectId")]
        public Int32 EffectId { get; set; }

        [SerializedName("itemEffectId")]
        public Int32 ItemEffectId { get; set; }

        [SerializedName("effect")]
        public Effect Effect { get; set; }

        [SerializedName("value")]
        public String Value { get; set; }

        [SerializedName("itemId")]
        public Int32 ItemId { get; set; }
    }
}
