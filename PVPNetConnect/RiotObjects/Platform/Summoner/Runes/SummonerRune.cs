using PVPNetConnect.RiotObjects.Platform.Catalog.Runes;
using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner.Runes
{
    public class SummonerRune : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.runes.SummonerRune";

        public SummonerRune()
        {
        }

        public SummonerRune(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerRune(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerRune result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("purchased")]
        public DateTime Purchased { get; set; }

        [InternalName("purchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [InternalName("runeId")]
        public Int32 RuneId { get; set; }

        [InternalName("quantity")]
        public Int32 Quantity { get; set; }

        [InternalName("rune")]
        public Rune Rune { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}