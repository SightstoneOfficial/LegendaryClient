using System;

namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    public class Inviter : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.gameinvite.contract.Inviter";

        public Inviter()
        {
        }

        public Inviter(Callback callback)
        {
            this.callback = callback;
        }

        public Inviter(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Inviter result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("previousSeasonHighestTier")]
        public String PreviousSeasonHighestTier { get; set; }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }

        [InternalName("summonerId")]
        public Int32 SummonerId { get; set; }

    }
}