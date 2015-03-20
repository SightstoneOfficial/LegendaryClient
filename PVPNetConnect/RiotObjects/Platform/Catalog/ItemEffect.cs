using System;

namespace PVPNetConnect.RiotObjects.Platform.Catalog
{
    public class ItemEffect : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.catalog.ItemEffect";

        public ItemEffect()
        {
        }

        public ItemEffect(Callback callback)
        {
            this.callback = callback;
        }

        public ItemEffect(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(ItemEffect result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("effectId")]
        public int EffectId { get; set; }

        [InternalName("itemEffectId")]
        public int ItemEffectId { get; set; }

        [InternalName("effect")]
        public Effect Effect { get; set; }

        [InternalName("value")]
        public string Value { get; set; }

        [InternalName("itemId")]
        public int ItemId { get; set; }
    }
}