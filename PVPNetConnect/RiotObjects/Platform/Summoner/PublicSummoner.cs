using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class PublicSummoner : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.PublicSummoner";

        public PublicSummoner()
        {
        }

        public PublicSummoner(Callback callback)
        {
            this.callback = callback;
        }

        public PublicSummoner(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PublicSummoner result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("internalName")]
        public String InternalName { get; set; }

        [InternalName("acctId")]
        public Double AcctId { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("profileIconId")]
        public Int32 ProfileIconId { get; set; }

        [InternalName("revisionDate")]
        public DateTime RevisionDate { get; set; }

        [InternalName("revisionId")]
        public Double RevisionId { get; set; }

        [InternalName("summonerLevel")]
        public Double SummonerLevel { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}