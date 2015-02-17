using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook
{
    public class SpellBookDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.spellbook.SpellBookDTO";

        public SpellBookDTO()
        {
        }

        public SpellBookDTO(Callback callback)
        {
            this.callback = callback;
        }

        public SpellBookDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SpellBookDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("defaultPage")]
        public SpellBookPageDTO DefaultPage { get; set; }

        [InternalName("bookPagesJson")]
        public object BookPagesJson { get; set; }

        [InternalName("bookPages")]
        public List<SpellBookPageDTO> BookPages { get; set; }

        [InternalName("dateString")]
        public String DateString { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}