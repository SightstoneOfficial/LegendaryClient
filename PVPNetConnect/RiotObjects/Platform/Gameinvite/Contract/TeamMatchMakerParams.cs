using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    public class MatchMakerParamsForTeam : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.matchmaking.MatchMakerParams";

        public MatchMakerParamsForTeam()
        {
        }

        public MatchMakerParamsForTeam(Callback callback)
        {
            this.callback = callback;
        }

        public MatchMakerParamsForTeam(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(MatchMakerParamsForTeam result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("lastMaestroMessage")]
        public object LastMaestroMessage { get; set; }

        [InternalName("teamId")]
        public object TeamId { get; set; }

        [InternalName("languages")]
        public object Languages { get; set; }

        [InternalName("botDifficulty")]
        public String BotDifficulty { get; set; }

        [InternalName("team")]
        public List<int> Team { get; set; }

        [InternalName("queueIds")]
        public List<Int32> QueueIds { get; set; }

        [InternalName("invitationId")]
        public object InvitationId { get; set; }
    }
}