using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class SummonerLevel : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.SummonerLevel";

        public SummonerLevel()
        {
        }

        public SummonerLevel(Callback callback)
        {
            this.callback = callback;
        }

        public SummonerLevel(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SummonerLevel result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("expTierMod")]
        public Double ExpTierMod { get; set; }

        [InternalName("grantRp")]
        public Double GrantRp { get; set; }

        [InternalName("expForLoss")]
        public Double ExpForLoss { get; set; }

        [InternalName("summonerTier")]
        public Double SummonerTier { get; set; }

        [InternalName("infTierMod")]
        public Double InfTierMod { get; set; }

        [InternalName("expToNextLevel")]
        public Double ExpToNextLevel { get; set; }

        [InternalName("expForWin")]
        public Double ExpForWin { get; set; }

        [InternalName("summonerLevel")]
        public Double Level { get; set; }
    }
}