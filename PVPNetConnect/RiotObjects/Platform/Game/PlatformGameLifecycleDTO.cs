using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class PlatformGameLifecycleDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.PlatformGameLifecycleDTO";

        public PlatformGameLifecycleDTO()
        {
        }

        public PlatformGameLifecycleDTO(Callback callback)
        {
            this.callback = callback;
        }

        public PlatformGameLifecycleDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PlatformGameLifecycleDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("gameSpecificLoyaltyRewards")]
        public object GameSpecificLoyaltyRewards { get; set; }

        [InternalName("reconnectDelay")]
        public Int32 ReconnectDelay { get; set; }

        [InternalName("lastModifiedDate")]
        public object LastModifiedDate { get; set; }

        [InternalName("game")]
        public GameDTO Game { get; set; }

        [InternalName("playerCredentials")]
        public PlayerCredentialsDto PlayerCredentials { get; set; }

        [InternalName("gameName")]
        public String GameName { get; set; }

        [InternalName("connectivityStateEnum")]
        public object ConnectivityStateEnum { get; set; }
    }
}