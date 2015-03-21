using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class ChampionBanInfoDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.ChampionBanInfoDTO";

        public ChampionBanInfoDTO(Callback callback)
        {
            this.callback = callback;
        }

        public ChampionBanInfoDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(ChampionBanInfoDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("enemyOwned")]
        public bool EnemyOwned { get; set; }

        [InternalName("championId")]
        public int ChampionId { get; set; }

        [InternalName("owned")]
        public bool Owned { get; set; }
    }
}