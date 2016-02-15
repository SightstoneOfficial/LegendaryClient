using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.GameObserver")]
    public class GameObserver
    {
        [SerializedName("accountId")]
        public double AccountId { get; set; }

        [SerializedName("botDifficulty")]
        public String BotDifficulty { get; set; }

        [SerializedName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [SerializedName("locale")]
        public object Locale { get; set; }

        [SerializedName("lastSelectedSkinIndex")]
        public int LastSelectedSkinIndex { get; set; }

        [SerializedName("partnerId")]
        public String PartnerId { get; set; }

        [SerializedName("profileIconId")]
        public int ProfileIconId { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }

        [SerializedName("badges")]
        public int Badges { get; set; }

        [SerializedName("pickTurn")]
        public int PickTurn { get; set; }

        [SerializedName("originalAccountId")]
        public double OriginalAccountId { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("pickMode")]
        public int PickMode { get; set; }

        [SerializedName("originalPlatformId")]
        public String OriginalPlatformId { get; set; }
    }
}
