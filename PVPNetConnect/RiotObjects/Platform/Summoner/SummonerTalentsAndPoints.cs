using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class SummonerTalentsAndPoints : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.SummonerTalentsAndPoints";

        public SummonerTalentsAndPoints()
        {
        }

        public SummonerTalentsAndPoints(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerTalentsAndPoints(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerTalentsAndPoints result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("talentPoints")]
        public Int32 TalentPoints { get; set; }

        [InternalName("modifyDate")]
        public DateTime ModifyDate { get; set; }

        [InternalName("createDate")]
        public DateTime CreateDate { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}