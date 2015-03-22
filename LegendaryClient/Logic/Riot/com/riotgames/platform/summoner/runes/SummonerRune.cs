using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.runes.SummonerRune")]
    public class SummonerRune
    {
        [SerializedName("purchased")]
        public DateTime Purchased { get; set; }

        [SerializedName("purchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [SerializedName("runeId")]
        public Int32 RuneId { get; set; }

        [SerializedName("quantity")]
        public Int32 Quantity { get; set; }

        [SerializedName("rune")]
        public Rune Rune { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }
    }
}
