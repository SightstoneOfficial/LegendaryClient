using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;
using System;

namespace PVPNetConnect.RiotObjects.Gameinvite.Contract
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

        [InternalName("inviter")]
        public Inviter inviter { get; set; }

        [InternalName("gameMetaData")]
        public String gameMetaData { get; set; }

        [InternalName("owner")]
        public String owner { get; set; }
        //public Owner owner { get; set; }

        [InternalName("invitationStateAsString")]
        public String invitationStateAsString { get; set; }

        [InternalName("invitationState")]
        public String invitationState { get; set; }

        [InternalName("invitationId")]
        public String InvitationID { get; set; }

    }
}