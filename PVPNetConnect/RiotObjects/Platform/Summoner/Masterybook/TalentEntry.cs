using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook
{
    public class TalentEntry : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.masterybook.TalentEntry";

        public TalentEntry()
        {
        }

        public TalentEntry(Callback callback)
        {
            this.callback = callback;
        }

        public TalentEntry(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TalentEntry result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("rank")]
        public Int32 Rank { get; set; }

        [InternalName("talentId")]
        public Int32 TalentId { get; set; }

        [InternalName("talent")]
        public Talent Talent { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}