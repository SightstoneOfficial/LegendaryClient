using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class ChampionMastery : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.ChampionMastery";
        //com.riotgames.platform.serviceproxy.dispatch.LcdsServiceProxyResponse

        public ChampionMastery()
        {
        }

        public ChampionMastery(Callback callback)
        {
            this.callback = callback;
        }

        public ChampionMastery(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(ChampionMastery result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("playerId")]
        public Double PlayerId { get; set; }

        [InternalName("championId")]
        public Int32 ChampionId { get; set; }

        [InternalName("championLevel")]
        public Int32 ChampionLevel { get; set; }

        [InternalName("championPoints")]
        public Int32 ChampionPoints { get; set; }

        [InternalName("lastPlayTime")]
        public Double LastPlayTime { get; set; }

        [InternalName("championPointsSinceLastLevel")]
        public Int32 ChampionPointsSinceLastLevel { get; set; }

        [InternalName("championPointsUntilNextLevel")]
        public Int32 ChampionPointsUntilNextLevel { get; set; }
    }
}