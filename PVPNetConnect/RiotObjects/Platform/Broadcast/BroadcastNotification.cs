namespace PVPNetConnect.RiotObjects.Platform.Broadcast
{
    public class BroadcastNotification : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.broadcast.BroadcastNotification";

        public BroadcastNotification()
        {
        }

        public BroadcastNotification(Callback callback)
        {
            this.callback = callback;
        }

        public BroadcastNotification(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(BroadcastNotification result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("broadcastMessages")]
        public object[] BroadcastMessages { get; set; }
    }
}