using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Catalog.Runes
{
    public class Rune : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.catalog.runes.Rune";

        public Rune()
        {
        }

        public Rune(Callback callback)
        {
            this.callback = callback;
        }

        public Rune(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Rune result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("imagePath")]
        public object ImagePath { get; set; }

        [InternalName("toolTip")]
        public object ToolTip { get; set; }

        [InternalName("tier")]
        public int Tier { get; set; }

        [InternalName("itemId")]
        public int ItemId { get; set; }

        [InternalName("runeType")]
        public RuneType RuneType { get; set; }

        [InternalName("duration")]
        public int Duration { get; set; }

        [InternalName("gameCode")]
        public int GameCode { get; set; }

        [InternalName("itemEffects")]
        public List<ItemEffect> ItemEffects { get; set; }

        [InternalName("baseType")]
        public string BaseType { get; set; }

        [InternalName("description")]
        public string Description { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("uses")]
        public object Uses { get; set; }
    }
}