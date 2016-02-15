using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.PracticeGameConfig")]
    public class PracticeGameConfig
    {
        [SerializedName("passbackUrl")]
        public object PassbackUrl { get; set; }

        [SerializedName("gameName")]
        public String GameName { get; set; }

        [SerializedName("gameTypeConfig")]
        public int GameTypeConfig { get; set; }

        [SerializedName("passbackDataPacket")]
        public object PassbackDataPacket { get; set; }

        [SerializedName("gamePassword")]
        public String GamePassword { get; set; }

        [SerializedName("gameMap")]
        public GameMap GameMap { get; set; }

        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("allowSpectators")]
        public String AllowSpectators { get; set; }

        [SerializedName("maxNumPlayers")]
        public int MaxNumPlayers { get; set; }

        [SerializedName("region")]
        public String Region { get; set; }
    }
}
