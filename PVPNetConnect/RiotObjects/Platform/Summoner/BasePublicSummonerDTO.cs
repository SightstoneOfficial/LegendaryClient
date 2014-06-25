using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class BasePublicSummonerDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.BasePublicSummonerDTO";

        public BasePublicSummonerDTO()
        {
        }

        public BasePublicSummonerDTO(Callback callback)
        {
            this.callback = callback;
        }

        public BasePublicSummonerDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(BasePublicSummonerDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("seasonTwoTier")]
        public String SeasonTwoTier { get; set; }

        [InternalName("internalName")]
        public String InternalName { get; set; }

        [InternalName("seasonOneTier")]
        public String SeasonOneTier { get; set; }

        [InternalName("acctId")]
        public Double AcctId { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("sumId")]
        public Double SumId { get; set; }

        [InternalName("profileIconId")]
        public Int32 ProfileIconId { get; set; }
    }
}