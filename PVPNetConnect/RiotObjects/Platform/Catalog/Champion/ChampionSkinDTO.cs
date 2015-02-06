using System;

namespace PVPNetConnect.RiotObjects.Platform.Catalog.Champion
{
    public class ChampionSkinDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.catalog.champion.ChampionSkinDTO";

        public ChampionSkinDTO()
        {
        }

        public ChampionSkinDTO(Callback callback)
        {
            this.callback = callback;
        }

        public ChampionSkinDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(ChampionSkinDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("championId")]
        public Int32 ChampionId { get; set; }

        [InternalName("skinId")]
        public Int32 SkinId { get; set; }

        [InternalName("freeToPlayReward")]
        public Boolean FreeToPlayReward { get; set; }

        [InternalName("stillObtainable")]
        public Boolean StillObtainable { get; set; }

        [InternalName("lastSelected")]
        public Boolean LastSelected { get; set; }

        [InternalName("skinIndex")]
        public Int32 SkinIndex { get; set; }

        [InternalName("owned")]
        public Boolean Owned { get; set; }

        [InternalName("winCountRemaining")]
        public Int32 WinCountRemaining { get; set; }

        [InternalName("purchaseDate")]
        public Double PurchaseDate { get; set; }

        [InternalName("endDate")]
        public Int32 EndDate { get; set; }
    }
}