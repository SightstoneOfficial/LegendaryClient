using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class ObfuscatedParticipant : Participant
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.ObfuscatedParticipant";

        public ObfuscatedParticipant()
        {
        }

        public ObfuscatedParticipant(Callback callback)
        {
            this.callback = callback;
        }

        public ObfuscatedParticipant(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public new delegate void Callback(ObfuscatedParticipant result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("badges")]
        public int Badges { get; set; }

        [InternalName("index")]
        public int Index { get; set; }

        [InternalName("clientInSynch")]
        public bool ClientInSynch { get; set; }

        [InternalName("gameUniqueId")]
        public int GameUniqueId { get; set; }

        [InternalName("pickMode")]
        public int PickMode { get; set; }
    }
}