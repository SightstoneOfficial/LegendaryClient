using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.BannedChampion")]
    public class BannedChampion
    {
        [SerializedName("pickTurn")]
        public int PickTurn { get; set; }

        [SerializedName("championId")]
        public int ChampionId { get; set; }

        [SerializedName("teamId")]
        public int TeamId { get; set; }
    }
}
