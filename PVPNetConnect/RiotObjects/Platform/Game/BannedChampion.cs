using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class BannedChampion : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.BannedChampion";

        public BannedChampion(Callback callback)
        {
            this.callback = callback;
        }

        public BannedChampion(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(BannedChampion result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [InternalName("championId")]
        public Int32 ChampionId { get; set; }

        [InternalName("teamId")]
        public Int32 TeamId { get; set; }
    }
}