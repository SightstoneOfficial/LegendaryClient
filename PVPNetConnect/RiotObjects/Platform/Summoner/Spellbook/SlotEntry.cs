using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner.Spellbook
{
    public class SlotEntry : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.spellbook.SlotEntry";

        public SlotEntry()
        {
        }

        public SlotEntry(Callback callback)
        {
            this.callback = callback;
        }

        public SlotEntry(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SlotEntry result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("runeId")]
        public Int32 RuneId { get; set; }

        [InternalName("runeSlotId")]
        public Int32 RuneSlotId { get; set; }
    }
}