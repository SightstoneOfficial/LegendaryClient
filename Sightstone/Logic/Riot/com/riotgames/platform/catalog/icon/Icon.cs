using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.catalog.icon.Icon")]
    public class Icon
    {
        [SerializedName("purchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [SerializedName("iconId")]
        public double IconId { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }
    }
}
