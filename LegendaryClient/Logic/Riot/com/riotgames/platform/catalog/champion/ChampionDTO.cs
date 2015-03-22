using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.catalog.champion.ChampionDTO")]
    public class ChampionDTO
    {
        [SerializedName("searchTags")]
        public String[] SearchTags { get; set; }

        [SerializedName("ownedByYourTeam")]
        public Boolean OwnedByYourTeam { get; set; }

        [SerializedName("botEnabled")]
        public Boolean BotEnabled { get; set; }

        [SerializedName("banned")]
        public Boolean Banned { get; set; }

        [SerializedName("skinName")]
        public String SkinName { get; set; }

        [SerializedName("displayName")]
        public String DisplayName { get; set; }

        [SerializedName("championData")]
        public object ChampionData { get; set; }

        [SerializedName("owned")]
        public Boolean Owned { get; set; }

        [SerializedName("championId")]
        public Int32 ChampionId { get; set; }

        [SerializedName("freeToPlayReward")]
        public Boolean FreeToPlayReward { get; set; }

        [SerializedName("freeToPlay")]
        public Boolean FreeToPlay { get; set; }

        [SerializedName("ownedByEnemyTeam")]
        public Boolean OwnedByEnemyTeam { get; set; }

        [SerializedName("active")]
        public Boolean Active { get; set; }

        [SerializedName("championSkins")]
        public List<ChampionSkinDTO> ChampionSkins { get; set; }

        [SerializedName("description")]
        public String Description { get; set; }

        [SerializedName("winCountRemaining")]
        public Int32 WinCountRemaining { get; set; }

        [SerializedName("purchaseDate")]
        public Double PurchaseDate { get; set; }

        [SerializedName("endDate")]
        public Int32 EndDate { get; set; }
    }
}
