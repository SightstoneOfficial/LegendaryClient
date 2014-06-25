using PVPNetConnect.RiotObjects.Platform.Game.Map;
using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class PracticeGameConfig : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.PracticeGameConfig";

        public PracticeGameConfig()
        {
        }

        public PracticeGameConfig(Callback callback)
        {
            this.callback = callback;
        }

        public PracticeGameConfig(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PracticeGameConfig result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("passbackUrl")]
        public object PassbackUrl { get; set; }

        [InternalName("gameName")]
        public String GameName { get; set; }

        [InternalName("gameTypeConfig")]
        public Int32 GameTypeConfig { get; set; }

        [InternalName("passbackDataPacket")]
        public object PassbackDataPacket { get; set; }

        [InternalName("gamePassword")]
        public String GamePassword { get; set; }

        [InternalName("gameMap")]
        public GameMap GameMap { get; set; }

        [InternalName("gameMode")]
        public String GameMode { get; set; }

        [InternalName("allowSpectators")]
        public String AllowSpectators { get; set; }

        [InternalName("maxNumPlayers")]
        public Int32 MaxNumPlayers { get; set; }

        [InternalName("region")]
        public String Region { get; set; }
    }
}