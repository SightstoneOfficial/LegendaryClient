using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.ChampionBanInfoDTO")]
    public class ChampionBanInfoDTO
    {
        [SerializedName("enemyOwned")]
        public bool EnemyOwned { get; set; }

        [SerializedName("championId")]
        public int ChampionId { get; set; }

        [SerializedName("owned")]
        public bool Owned { get; set; }
    }
}
