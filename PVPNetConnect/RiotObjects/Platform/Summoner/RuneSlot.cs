using PVPNetConnect.RiotObjects.Platform.Catalog.Runes;
using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class RuneSlot : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.RuneSlot";

        public RuneSlot()
        {
        }

        public RuneSlot(Callback callback)
        {
            this.callback = callback;
        }

        public RuneSlot(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(RuneSlot result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("id")]
        public Int32 Id { get; set; }

        [InternalName("minLevel")]
        public Int32 MinLevel { get; set; }

        [InternalName("runeType")]
        public RuneType RuneType { get; set; }
    }
}