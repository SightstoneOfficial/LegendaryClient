using System;

namespace PVPNetConnect.RiotObjects.Platform.Broadcast
{
    public class BroadcastMessage : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.broadcast.BroadcastMessage";

        public BroadcastMessage()
        {
        }

        public BroadcastMessage(Callback callback)
        {
            this.callback = callback;
        }

        public BroadcastMessage(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(BroadcastMessage result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("id")]
        public Int32 ID { get; set; }

        [InternalName("content")]
        public String Content { get; set; }

        [InternalName("severity")]
        public String Severity { get; set; }

        [InternalName("messageKey")]
        public String MessageKey { get; set; }

        [InternalName("active")]
        public bool Active { get; set; }
    }
}
