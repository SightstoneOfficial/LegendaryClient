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
        public string BotDifficulty { get; set; }

        [InternalName("summonerInternalName")]
        public string SummonerInternalName { get; set; }

        [InternalName("locale")]
        public object Locale { get; set; }

        [InternalName("lastSelectedSkinIndex")]
        public int LastSelectedSkinIndex { get; set; }

        [InternalName("partnerId")]
        public string PartnerId { get; set; }

        [InternalName("profileIconId")]
        public int ProfileIconId { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }

        [InternalName("badges")]
        public int Badges { get; set; }

        [InternalName("pickTurn")]
        public int PickTurn { get; set; }

        [InternalName("originalAccountId")]
        public Double OriginalAccountId { get; set; }

        [InternalName("summonerName")]
        public string SummonerName { get; set; }

        [InternalName("pickMode")]
        public int PickMode { get; set; }

        [InternalName("originalPlatformId")]
        public string OriginalPlatformId { get; set; }
    }
}