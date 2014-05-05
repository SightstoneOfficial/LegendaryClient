using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook
{
    public class MasteryBookPageDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.masterybook.MasteryBookPageDTO";

        public MasteryBookPageDTO()
        {
        }

        public MasteryBookPageDTO(Callback callback)
        {
            this.callback = callback;
        }

        public MasteryBookPageDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(MasteryBookPageDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("talentEntries")]
        public List<TalentEntry> TalentEntries { get; set; }

        [InternalName("pageId")]
        public Double PageId { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("current")]
        public Boolean Current { get; set; }

        [InternalName("createDate")]
        public object CreateDate { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}