using System;

namespace PVPNetConnect.RiotObjects.Platform.Messaging.Persistence
{
    public class SimpleDialogMessageResponse : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.messaging.persistence.SimpleDialogMessageResponse";

        public SimpleDialogMessageResponse()
        {
        }

        public SimpleDialogMessageResponse(Callback callback)
        {
            this.callback = callback;
        }

        public SimpleDialogMessageResponse(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SimpleDialogMessageResponse result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("command")]
        public string Command { get; set; }

        [InternalName("accountId")]
        public Double AccountId { get; set; }

        [InternalName("msgId")]
        public Double MessageId { get; set; }

    }
}