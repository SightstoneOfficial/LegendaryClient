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
        public string SeasonTwoTier { get; set; }

        [InternalName("internalName")]
        public string InternalName { get; set; }

        [InternalName("seasonOneTier")]
        public string SeasonOneTier { get; set; }

        [InternalName("acctId")]
        public double AcctId { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("sumId")]
        public double SumId { get; set; }

        [InternalName("profileIconId")]
        public int ProfileIconId { get; set; }
    }
}