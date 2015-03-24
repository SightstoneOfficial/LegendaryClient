using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.GameTypeConfigDTO")]
    public class GameTypeConfigDTO
    {
        [SerializedName("id")]
        public int Id { get; set; }

        [SerializedName("allowTrades")]
        public bool AllowTrades { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("mainPickTimerDuration")]
        public int MainPickTimerDuration { get; set; }

        [SerializedName("exclusivePick")]
        public bool ExclusivePick { get; set; }

        [SerializedName("duplicatePick")]
        public bool DuplicatePick { get; set; }

        [SerializedName("teamChampionPool")]
        public bool TeamChampionPool { get; set; }

        [SerializedName("pickMode")]
        public String PickMode { get; set; }

        [SerializedName("maxAllowableBans")]
        public int MaxAllowableBans { get; set; }

        [SerializedName("banTimerDuration")]
        public int BanTimerDuration { get; set; }

        [SerializedName("postPickTimerDuration")]
        public int PostPickTimerDuration { get; set; }
    }
}
