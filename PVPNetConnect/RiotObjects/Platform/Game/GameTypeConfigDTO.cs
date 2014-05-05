using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class GameTypeConfigDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.GameTypeConfigDTO";

        public GameTypeConfigDTO()
        {
        }

        public GameTypeConfigDTO(Callback callback)
        {
            this.callback = callback;
        }

        public GameTypeConfigDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(GameTypeConfigDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("id")]
        public Int32 Id { get; set; }

        [InternalName("allowTrades")]
        public Boolean AllowTrades { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("mainPickTimerDuration")]
        public Int32 MainPickTimerDuration { get; set; }

        [InternalName("exclusivePick")]
        public Boolean ExclusivePick { get; set; }

        [InternalName("duplicatePick")]
        public Boolean DuplicatePick { get; set; }

        [InternalName("teamChampionPool")]
        public Boolean TeamChampionPool { get; set; }

        [InternalName("pickMode")]
        public String PickMode { get; set; }

        [InternalName("maxAllowableBans")]
        public Int32 MaxAllowableBans { get; set; }

        [InternalName("banTimerDuration")]
        public Int32 BanTimerDuration { get; set; }

        [InternalName("postPickTimerDuration")]
        public Int32 PostPickTimerDuration { get; set; }
    }
}