using System;

namespace PVPNetConnect.RiotObjects.Platform.Messaging
{
    public class ClientLoginKickNotification : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.messaging.ClientLoginKickNotification";

        public ClientLoginKickNotification()
        {
        }

        public ClientLoginKickNotification(Callback callback)
        {
            this.callback = callback;
        }

        public ClientLoginKickNotification(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(ClientLoginKickNotification result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("sessionToken")]
        public String seesionToken { get; set; }
    }
}