using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.GameObserver")]
    public class GameObserver
    {
        [SerializedName("accountId")]
        public Double AccountId { get; set; }

        [SerializedName("botDifficulty")]
        public String BotDifficulty { get; set; }

        [SerializedName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [SerializedName("locale")]
        public object Locale { get; set; }

        [SerializedName("lastSelectedSkinIndex")]
        public Int32 LastSelectedSkinIndex { get; set; }

        [SerializedName("partnerId")]
        public String PartnerId { get; set; }

        [SerializedName("profileIconId")]
        public Int32 ProfileIconId { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }

        [SerializedName("badges")]
        public Int32 Badges { get; set; }

        [SerializedName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [SerializedName("originalAccountId")]
        public Double OriginalAccountId { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("pickMode")]
        public Int32 PickMode { get; set; }

        [SerializedName("originalPlatformId")]
        public String OriginalPlatformId { get; set; }
    }
}
