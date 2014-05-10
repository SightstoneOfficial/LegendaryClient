using System;

namespace PVPNetConnect.RiotObjects.Platform.Harassment
{
    public class LcdsResponseString : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.harassment.LcdsResponseString";

        public LcdsResponseString()
        {
        }

        public LcdsResponseString(Callback callback)
        {
            this.callback = callback;
        }

        public LcdsResponseString(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(LcdsResponseString result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("value")]
        public String Value { get; set; }
    }
}