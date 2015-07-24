using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.gameinvite.contract.InvitationRequest")]
    public class InvitationRequest
    {
        [SerializedName("gameMetaData")]
        public string GameMetaData { get; set; }

        [SerializedName("invitationStateAsString")]
        public string InvitationStateAsString { get; set; }

        [SerializedName("invitationState")]
        public string InvitationState { get; set; }

        [SerializedName("invitationId")]
        public string InvitationId { get; set; }

        [SerializedName("inviter")]
        public Inviter Inviter { get; set; }

        [SerializedName("owner")]
        public Player Owner { get; set; }
    }
}
