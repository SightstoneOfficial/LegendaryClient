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
        public string InternalName { get; set; }

        [InternalName("acctId")]
        public double AcctId { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("profileIconId")]
        public int ProfileIconId { get; set; }

        [InternalName("revisionDate")]
        public DateTime RevisionDate { get; set; }

        [InternalName("revisionId")]
        public double RevisionId { get; set; }

        [InternalName("summonerLevel")]
        public double SummonerLevel { get; set; }

        [InternalName("summonerId")]
        public double SummonerId { get; set; }
    }
}