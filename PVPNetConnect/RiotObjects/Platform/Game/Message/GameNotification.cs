using System;

namespace PVPNetConnect.RiotObjects.Platform.Game.Message
{
    public class GameNotification : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.message.GameNotification";

        public GameNotification()
        {
        }

        public GameNotification(Callback callback)
        {
            this.callback = callback;
        }

        public GameNotification(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(GameNotification result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("messageCode")]
        public string MessageCode { get; set; }

        [InternalName("type")]
        public string Type { get; set; }

        [InternalName("messageArgument")]
        public object MessageArgument { get; set; }
    }
}