using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.catalog.champion.ChampionSkinDTO")]
    public class ChampionSkinDTO
    {
        [SerializedName("championId")]
        public int ChampionId { get; set; }

        [SerializedName("skinId")]
        public int SkinId { get; set; }

        [SerializedName("freeToPlayReward")]
        public bool FreeToPlayReward { get; set; }

        [SerializedName("stillObtainable")]
        public bool StillObtainable { get; set; }

        [SerializedName("lastSelected")]
        public bool LastSelected { get; set; }

        [SerializedName("skinIndex")]
        public int SkinIndex { get; set; }

        [SerializedName("owned")]
        public bool Owned { get; set; }

        [SerializedName("winCountRemaining")]
        public int WinCountRemaining { get; set; }

        [SerializedName("purchaseDate")]
        public double PurchaseDate { get; set; }

        [SerializedName("endDate")]
        public double EndDate { get; set; }
    }
}
