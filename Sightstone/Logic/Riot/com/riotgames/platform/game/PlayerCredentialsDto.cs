using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.PlayerCredentialsDto")]
    public class PlayerCredentialsDto
    {
        [SerializedName("encryptionKey")]
        public String EncryptionKey { get; set; }

        [SerializedName("gameId")]
        public double GameId { get; set; }

        [SerializedName("lastSelectedSkinIndex")]
        public int LastSelectedSkinIndex { get; set; }

        [SerializedName("serverIp")]
        public String ServerIp { get; set; }

        [SerializedName("observer")]
        public bool Observer { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }

        [SerializedName("observerServerIp")]
        public String ObserverServerIp { get; set; }

        [SerializedName("handshakeToken")]
        public String HandshakeToken { get; set; }

        [SerializedName("playerId")]
        public double PlayerId { get; set; }

        [SerializedName("serverPort")]
        public int ServerPort { get; set; }

        [SerializedName("observerServerPort")]
        public int ObserverServerPort { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("observerEncryptionKey")]
        public String ObserverEncryptionKey { get; set; }

        [SerializedName("championId")]
        public int ChampionId { get; set; }
    }
}
