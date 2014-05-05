using PVPNetConnect.RiotObjects.Platform.Catalog.Runes;
using System;

namespace PVPNetConnect.RiotObjects.Platform.Catalog
{
    public class Effect : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.catalog.Effect";

        public Effect()
        {
        }

        public Effect(Callback callback)
        {
            this.callback = callback;
        }

        public Effect(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Effect result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("effectId")]
        public Int32 EffectId { get; set; }

        [InternalName("gameCode")]
        public String GameCode { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("categoryId")]
        public object CategoryId { get; set; }

        [InternalName("runeType")]
        public RuneType RuneType { get; set; }
    }
}