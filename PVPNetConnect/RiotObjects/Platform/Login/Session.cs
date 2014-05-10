using PVPNetConnect.RiotObjects.Platform.Account;
using System;

namespace PVPNetConnect.RiotObjects.Platform.Login
{
    public class Session : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.login.Session";

        public Session()
        {
        }

        public Session(Callback callback)
        {
            this.callback = callback;
        }

        public Session(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Session result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("token")]
        public String Token { get; set; }

        [InternalName("password")]
        public String Password { get; set; }

        [InternalName("accountSummary")]
        public AccountSummary AccountSummary { get; set; }
    }
}