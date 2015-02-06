using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Catalog.Champion
{
    public class ChampionDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.catalog.champion.ChampionDTO";

        public ChampionDTO()
        {
        }

        public ChampionDTO(Callback callback)
        {
            this.callback = callback;
        }

        public ChampionDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(ChampionDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("searchTags")]
        public String[] SearchTags { get; set; }

        [InternalName("secondarySearchTags")]
        public String[] secondarySearchTags { get; set; }

        [InternalName("ownedByYourTeam")]
        public Boolean OwnedByYourTeam { get; set; }

        [InternalName("botEnabled")]
        public Boolean BotEnabled { get; set; }

        [InternalName("banned")]
        public Boolean Banned { get; set; }

        [InternalName("skinName")]
        public String SkinName { get; set; }

        [InternalName("displayName")]
        public String DisplayName { get; set; }

        [InternalName("championData")]
        public TypedObject ChampionData { get; set; }

        [InternalName("owned")]
        public Boolean Owned { get; set; }

        [InternalName("championId")]
        public Int32 ChampionId { get; set; }

        [InternalName("freeToPlayReward")]
        public Boolean FreeToPlayReward { get; set; }

        [InternalName("freeToPlay")]
        public Boolean FreeToPlay { get; set; }

        [InternalName("ownedByEnemyTeam")]
        public Boolean OwnedByEnemyTeam { get; set; }

        [InternalName("active")]
        public Boolean Active { get; set; }

        [InternalName("championSkins")]
        public List<ChampionSkinDTO> ChampionSkins { get; set; }

        [InternalName("description")]
        public String Description { get; set; }

        [InternalName("winCountRemaining")]
        public Int32 WinCountRemaining { get; set; }

        [InternalName("purchaseDate")]
        public Double PurchaseDate { get; set; }

        [InternalName("endDate")]
        public Int32 EndDate { get; set; }
    }
}