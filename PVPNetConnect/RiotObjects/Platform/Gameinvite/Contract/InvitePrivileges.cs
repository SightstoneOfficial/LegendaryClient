using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    public class InvitePrivileges : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.gameinvite.contract.InvitePrivileges";

        public InvitePrivileges()
        {
        }

        public InvitePrivileges(Callback callback)
        {
            this.callback = callback;
        }

        public InvitePrivileges(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(InvitePrivileges result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("canInvite")]
        public bool canInvite { get; set; }
    }
}