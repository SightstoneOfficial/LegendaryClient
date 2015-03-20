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
        public string[] SearchTags { get; set; }

        [InternalName("secondarySearchTags")]
        public string[] secondarySearchTags { get; set; }

        [InternalName("ownedByYourTeam")]
        public bool OwnedByYourTeam { get; set; }

        [InternalName("botEnabled")]
        public bool BotEnabled { get; set; }

        [InternalName("banned")]
        public bool Banned { get; set; }

        [InternalName("skinName")]
        public string SkinName { get; set; }

        [InternalName("displayName")]
        public string DisplayName { get; set; }

        [InternalName("championData")]
        public TypedObject ChampionData { get; set; }

        [InternalName("owned")]
        public bool Owned { get; set; }

        [InternalName("championId")]
        public int ChampionId { get; set; }

        [InternalName("freeToPlayReward")]
        public bool FreeToPlayReward { get; set; }

        [InternalName("freeToPlay")]
        public bool FreeToPlay { get; set; }

        [InternalName("ownedByEnemyTeam")]
        public bool OwnedByEnemyTeam { get; set; }

        [InternalName("active")]
        public bool Active { get; set; }

        [InternalName("championSkins")]
        public List<ChampionSkinDTO> ChampionSkins { get; set; }

        [InternalName("description")]
        public string Description { get; set; }

        [InternalName("winCountRemaining")]
        public int WinCountRemaining { get; set; }

        [InternalName("purchaseDate")]
        public double PurchaseDate { get; set; }

        [InternalName("endDate")]
        public int EndDate { get; set; }
    }
}