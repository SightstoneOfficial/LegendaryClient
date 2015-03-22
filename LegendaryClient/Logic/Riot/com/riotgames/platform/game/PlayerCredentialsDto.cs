using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.PlayerCredentialsDto")]
    public class PlayerCredentialsDto
    {
        [SerializedName("encryptionKey")]
        public String EncryptionKey { get; set; }

        [SerializedName("gameId")]
        public Double GameId { get; set; }

        [SerializedName("lastSelectedSkinIndex")]
        public Int32 LastSelectedSkinIndex { get; set; }

        [SerializedName("serverIp")]
        public String ServerIp { get; set; }

        [SerializedName("observer")]
        public Boolean Observer { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }

        [SerializedName("observerServerIp")]
        public String ObserverServerIp { get; set; }

        [SerializedName("handshakeToken")]
        public String HandshakeToken { get; set; }

        [SerializedName("playerId")]
        public Double PlayerId { get; set; }

        [SerializedName("serverPort")]
        public Int32 ServerPort { get; set; }

        [SerializedName("observerServerPort")]
        public Int32 ObserverServerPort { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("observerEncryptionKey")]
        public String ObserverEncryptionKey { get; set; }

        [SerializedName("championId")]
        public Int32 ChampionId { get; set; }
    }
}
