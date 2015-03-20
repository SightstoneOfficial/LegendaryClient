using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    public class InvitationRequest : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.gameinvite.contract.InvitationRequest";

        public InvitationRequest()
        {
        }

        public InvitationRequest(Callback callback)
        {
            this.callback = callback;
        }

        public InvitationRequest(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(InvitationRequest result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("gameMetaData")]
        public string GameMetaData { get; set; }

        [InternalName("invitationStateAsString")]
        public string InvitationStateAsString { get; set; }

        [InternalName("invitationState")]
        public string InvitationState { get; set; }

        [InternalName("invitationId")]
        public string InvitationId { get; set; }

        [InternalName("inviter")]
        public Inviter Inviter { get; set; }

        [InternalName("owner")]
        public Player Owner { get; set; }



    }
}