using System;

namespace PVPNetConnect.RiotObjects.Platform.Messaging.Persistence
{
    public class SimpleDialogMessage : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.messaging.persistence.SimpleDialogMessage";

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
        public string TitleCode { get; set; }

        [InternalName("accountId")]
        public Double AccountId { get; set; }

        [InternalName("params")]
        public object Params { get; set; }

        [InternalName("msgId")]
        public Double MessageId { get; set; }

        [InternalName("type")]
        public string Type { get; set; }

        [InternalName("bodyCode")]
        public string BodyCode { get; set; }
    }
}