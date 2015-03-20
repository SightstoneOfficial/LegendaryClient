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
        public int ChampionId { get; set; }

        [InternalName("championLevel")]
        public int ChampionLevel { get; set; }

        [InternalName("championPoints")]
        public int ChampionPoints { get; set; }

        [InternalName("lastPlayTime")]
        public Double LastPlayTime { get; set; }

        [InternalName("championPointsSinceLastLevel")]
        public int ChampionPointsSinceLastLevel { get; set; }

        [InternalName("championPointsUntilNextLevel")]
        public int ChampionPointsUntilNextLevel { get; set; }
    }
}