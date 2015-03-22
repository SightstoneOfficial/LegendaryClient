using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.catalog.champion.ChampionSkinDTO")]
    public class ChampionSkinDTO
    {
        [SerializedName("championId")]
        public Int32 ChampionId { get; set; }

        [SerializedName("skinId")]
        public Int32 SkinId { get; set; }

        [SerializedName("freeToPlayReward")]
        public Boolean FreeToPlayReward { get; set; }

        [SerializedName("stillObtainable")]
        public Boolean StillObtainable { get; set; }

        [SerializedName("lastSelected")]
        public Boolean LastSelected { get; set; }

        [SerializedName("skinIndex")]
        public Int32 SkinIndex { get; set; }

        [SerializedName("owned")]
        public Boolean Owned { get; set; }

        [SerializedName("winCountRemaining")]
        public Int32 WinCountRemaining { get; set; }

        [SerializedName("purchaseDate")]
        public Double PurchaseDate { get; set; }

        [SerializedName("endDate")]
        public Double EndDate { get; set; }
    }
}
