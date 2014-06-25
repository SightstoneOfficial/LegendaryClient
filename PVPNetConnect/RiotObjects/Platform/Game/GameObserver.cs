using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class GameObserver : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.GameObserver";

        public GameObserver()
        {
        }

        public GameObserver(Callback callback)
        {
            this.callback = callback;
        }

        public GameObserver(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(GameObserver result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("accountId")]
        public Double AccountId { get; set; }

        [InternalName("botDifficulty")]
        public String BotDifficulty { get; set; }

        [InternalName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [InternalName("locale")]
        public object Locale { get; set; }

        [InternalName("lastSelectedSkinIndex")]
        public Int32 LastSelectedSkinIndex { get; set; }

        [InternalName("partnerId")]
        public String PartnerId { get; set; }

        [InternalName("profileIconId")]
        public Int32 ProfileIconId { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }

        [InternalName("badges")]
        public Int32 Badges { get; set; }

        [InternalName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [InternalName("originalAccountId")]
        public Double OriginalAccountId { get; set; }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }

        [InternalName("pickMode")]
        public Int32 PickMode { get; set; }

        [InternalName("originalPlatformId")]
        public String OriginalPlatformId { get; set; }
    }
}