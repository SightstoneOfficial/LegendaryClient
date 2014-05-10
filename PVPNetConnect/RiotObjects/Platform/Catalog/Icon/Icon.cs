using System;

namespace PVPNetConnect.RiotObjects.Platform.Catalog.Icon
{
    public class Icon : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.catalog.icon.Icon";

        public Icon()
        {
        }

        public Icon(Callback callback)
        {
            this.callback = callback;
        }

        public Icon(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Icon result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("purchaseDate")]
        public DateTime PurchaseDate { get; set; }

        [InternalName("iconId")]
        public Double IconId { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}