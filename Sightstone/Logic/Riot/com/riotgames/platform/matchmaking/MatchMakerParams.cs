using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.matchmaking.MatchMakerParams")]
    public class MatchMakerParams
    {
        [SerializedName("lastMaestroMessage")]
        public object LastMaestroMessage { get; set; }

        [SerializedName("teamId")]
        public object TeamId { get; set; }

        [SerializedName("languages")]
        public object Languages { get; set; }

        [SerializedName("botDifficulty")]
        public String BotDifficulty { get; set; }

        [SerializedName("team")]
        public List<int> Team { get; set; }

        [SerializedName("queueIds")]
        public int[] QueueIds { get; set; }

        [SerializedName("invitationId")]
        public object InvitationId { get; set; }
    }
}
