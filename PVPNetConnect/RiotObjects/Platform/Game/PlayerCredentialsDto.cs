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
        public double GameId { get; set; }

        [InternalName("lastSelectedSkinIndex")]
        public int LastSelectedSkinIndex { get; set; }

        [InternalName("dataVersion")]
        public new int DataVersion { get; set; }

        [InternalName("serverIp")]
        public string ServerIp { get; set; }

        [InternalName("observer")]
        public bool Observer { get; set; }

        [InternalName("futureData")]
        public new bool FutureData { get; set; }

        [InternalName("summonerId")]
        public double SummonerId { get; set; }

        [InternalName("observerServerIp")]
        public string ObserverServerIp { get; set; }

        [InternalName("handshakeToken")]
        public string HandshakeToken { get; set; }

        [InternalName("playerId")]
        public double PlayerId { get; set; }

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