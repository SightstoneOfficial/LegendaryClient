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
        public String EncryptionKey { get; set; }

        [InternalName("gameId")]
        public Double GameId { get; set; }

        [InternalName("lastSelectedSkinIndex")]
        public Int32 LastSelectedSkinIndex { get; set; }

        [InternalName("dataVersion")]
        public new Int32 DataVersion { get; set; }

        [InternalName("serverIp")]
        public String ServerIp { get; set; }

        [InternalName("observer")]
        public Boolean Observer { get; set; }

        [InternalName("futureData")]
        public new Boolean FutureData { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }

        [InternalName("observerServerIp")]
        public String ObserverServerIp { get; set; }

        [InternalName("handshakeToken")]
        public String HandshakeToken { get; set; }

        [InternalName("playerId")]
        public Double PlayerId { get; set; }

        [InternalName("serverPort")]
        public Int32 ServerPort { get; set; }

        [InternalName("observerServerPort")]
        public Int32 ObserverServerPort { get; set; }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }

        [InternalName("observerEncryptionKey")]
        public String ObserverEncryptionKey { get; set; }

        [InternalName("championId")]
        public Int32 ChampionId { get; set; }
    }
}