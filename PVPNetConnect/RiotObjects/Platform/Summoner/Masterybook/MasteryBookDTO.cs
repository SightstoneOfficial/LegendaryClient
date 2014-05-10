using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Summoner.Masterybook
{
    public class MasteryBookDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.masterybook.MasteryBookDTO";

        public MasteryBookDTO()
        {
        }

        public MasteryBookDTO(Callback callback)
        {
            this.callback = callback;
        }

        public MasteryBookDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(MasteryBookDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("bookPagesJson")]
        public object BookPagesJson { get; set; }

        [InternalName("bookPages")]
        public List<MasteryBookPageDTO> BookPages { get; set; }

        [InternalName("dateString")]
        public String DateString { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}