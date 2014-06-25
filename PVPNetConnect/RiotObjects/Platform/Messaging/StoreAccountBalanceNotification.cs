using System;

namespace PVPNetConnect.RiotObjects.Platform.Messaging
{
    public class StoreAccountBalanceNotification : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.reroll.pojo.StoreAccountBalanceNotification";

        public StoreAccountBalanceNotification()
        {
        }

        public StoreAccountBalanceNotification(Callback callback)
        {
            this.callback = callback;
        }

        public StoreAccountBalanceNotification(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(StoreAccountBalanceNotification result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("rp")]
        public Double Rp { get; set; }

        [InternalName("ip")]
        public Double Ip { get; set; }
    }
}