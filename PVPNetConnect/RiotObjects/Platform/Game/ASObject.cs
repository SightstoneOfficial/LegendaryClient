using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class ASObject : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "ASObject";

        public ASObject()
        {
        }

        public ASObject(Callback callback)
        {
            this.callback = callback;
        }

        public ASObject(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(ASObject result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("LEAVER_BUSTER_ACCESS_TOKEN")]
        public string Token { get; set; }

        [InternalName("TypeName")]
        public object Tname { get; set; }

    }
}
