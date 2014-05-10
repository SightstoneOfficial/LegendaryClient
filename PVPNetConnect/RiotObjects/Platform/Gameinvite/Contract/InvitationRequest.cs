using System;

namespace PVPNetConnect.RiotObjects.Gameinvite.Contract
{
    public class InvitationRequest : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.leagues.gameinvite.contract.InvitationRequest";

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
        public Int32 inviter { get; set; }

    }
}