using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner.Boost
{
    public class SummonerActiveBoostsDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.boost.SummonerActiveBoostsDTO";

        public SummonerActiveBoostsDTO()
        {
        }

        public SummonerActiveBoostsDTO(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerActiveBoostsDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerActiveBoostsDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("xpBoostEndDate")]
        public Double XpBoostEndDate { get; set; }

        [InternalName("xpBoostPerWinCount")]
        public Int32 XpBoostPerWinCount { get; set; }

        [InternalName("xpLoyaltyBoost")]
        public Int32 XpLoyaltyBoost { get; set; }

        [InternalName("ipBoostPerWinCount")]
        public Int32 IpBoostPerWinCount { get; set; }

        [InternalName("ipLoyaltyBoost")]
        public Int32 IpLoyaltyBoost { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }

        [InternalName("ipBoostEndDate")]
        public Double IpBoostEndDate { get; set; }
    }
}