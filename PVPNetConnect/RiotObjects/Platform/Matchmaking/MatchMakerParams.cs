using System;
using System.Collections.Generic;
using PVPNetConnect.RiotObjects.Team;

namespace PVPNetConnect.RiotObjects.Platform.Matchmaking
{
    public class MatchMakerParams : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.matchmaking.MatchMakerParams";

        public MatchMakerParams()
        {
        }

        public MatchMakerParams(Callback callback)
        {
            this.callback = callback;
        }

        public MatchMakerParams(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(MatchMakerParams result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("lastMaestroMessage")]
        public object LastMaestroMessage { get; set; }

        [InternalName("teamId")]
        public TeamId TeamId { get; set; }

        [InternalName("languages")]
        public object Languages { get; set; }

        [InternalName("botDifficulty")]
        public String BotDifficulty { get; set; }

        [InternalName("team")]
        public List<int> Team { get; set; }

        [InternalName("queueIds")]
        public Int32[] QueueIds { get; set; }

        [InternalName("invitationId")]
        public object InvitationId { get; set; }
    }
}