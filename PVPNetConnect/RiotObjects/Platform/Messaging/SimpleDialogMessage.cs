using System;

namespace PVPNetConnect.RiotObjects.Platform.Messaging
{
    public class SimpleDialogMessage : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.reroll.pojo.SimpleDialogMessage";

        public SimpleDialogMessage()
        {
        }

        public SimpleDialogMessage(Callback callback)
        {
            this.callback = callback;
        }

        public SimpleDialogMessage(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(SimpleDialogMessage result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("titleCode")]
        public String TitleCode { get; set; }

        [InternalName("accountId")]
        public Double AccountId { get; set; }

        [InternalName("params")]
        public object Params { get; set; }

        [InternalName("type")]
        public String Type { get; set; }
    }
}