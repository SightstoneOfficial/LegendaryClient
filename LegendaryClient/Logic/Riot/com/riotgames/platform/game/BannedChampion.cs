using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.BannedChampion")]
    public class BannedChampion
    {
        [SerializedName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [SerializedName("championId")]
        public Int32 ChampionId { get; set; }

        [SerializedName("teamId")]
        public Int32 TeamId { get; set; }
    }
}
