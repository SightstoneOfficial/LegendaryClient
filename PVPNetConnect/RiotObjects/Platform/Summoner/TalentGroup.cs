using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class TalentGroup : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.TalentGroup";

        public TalentGroup()
        {
        }

        public TalentGroup(Callback callback)
        {
            this.callback = callback;
        }

        public TalentGroup(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TalentGroup result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("index")]
        public Int32 Index { get; set; }

        [InternalName("talentRows")]
        public List<TalentRow> TalentRows { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("tltGroupId")]
        public Int32 TltGroupId { get; set; }
    }
}