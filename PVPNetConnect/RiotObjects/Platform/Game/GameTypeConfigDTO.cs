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
        public int Id { get; set; }

        [InternalName("allowTrades")]
        public bool AllowTrades { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("mainPickTimerDuration")]
        public int MainPickTimerDuration { get; set; }

        [InternalName("exclusivePick")]
        public bool ExclusivePick { get; set; }

        [InternalName("duplicatePick")]
        public bool DuplicatePick { get; set; }

        [InternalName("crossTeamChampionPool")]
        public bool CrossTeamChampionPool { get; set; }

        [InternalName("teamChampionPool")]
        public bool TeamChampionPool { get; set; }

        [InternalName("pickMode")]
        public string PickMode { get; set; }

        [InternalName("maxAllowableBans")]
        public int MaxAllowableBans { get; set; }

        [InternalName("banTimerDuration")]
        public int BanTimerDuration { get; set; }

        [InternalName("postPickTimerDuration")]
        public int PostPickTimerDuration { get; set; }
    }
}