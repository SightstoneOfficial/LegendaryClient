using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class PlayerCredentialsDto : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.PlayerCredentialsDto";

        public PlayerCredentialsDto()
        {
        }

        public PlayerCredentialsDto(Callback callback)
        {
            this.callback = callback;
        }

        public PlayerCredentialsDto(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PlayerCredentialsDto result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("encryptionKey")]
        public string EncryptionKey { get; set; }

        [InternalName("gameId")]
        public Double GameId { get; set; }

        [InternalName("lastSelectedSkinIndex")]
        public int LastSelectedSkinIndex { get; set; }

        [InternalName("dataVersion")]
        public new int DataVersion { get; set; }

        [InternalName("serverIp")]
        public string ServerIp { get; set; }

        [InternalName("observer")]
        public Boolean Observer { get; set; }

        [InternalName("futureData")]
        public new Boolean FutureData { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }

        [InternalName("observerServerIp")]
        public string ObserverServerIp { get; set; }

        [InternalName("handshakeToken")]
        public string HandshakeToken { get; set; }

        [InternalName("playerId")]
        public Double PlayerId { get; set; }

        [InternalName("serverPort")]
        public int ServerPort { get; set; }

        [InternalName("observerServerPort")]
        public int ObserverServerPort { get; set; }

        [InternalName("summonerName")]
        public string SummonerName { get; set; }

        [InternalName("observerEncryptionKey")]
        public string ObserverEncryptionKey { get; set; }

        [InternalName("championId")]
        public int ChampionId { get; set; }
    }
}