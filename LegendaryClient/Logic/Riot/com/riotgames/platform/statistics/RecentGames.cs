using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.RecentGames")]
    public class RecentGames
    {
        [SerializedName("recentGamesJson")]
        public object RecentGamesJson { get; set; }

        [SerializedName("playerGameStatsMap")]
        public object PlayerGameStatsMap { get; set; }

        [SerializedName("gameStatistics")]
        public List<PlayerGameStats> GameStatistics { get; set; }

        [SerializedName("userId")]
        public double UserId { get; set; }
    }
}
