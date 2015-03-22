using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Team
{
    [Serializable]
    [SerializedName("com.riotgames.team.stats.MatchHistorySummary")]
    public class MatchHistorySummary
    {
        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("mapId")]
        public Int32 MapId { get; set; }

        [SerializedName("assists")]
        public Int32 Assists { get; set; }

        [SerializedName("opposingTeamName")]
        public String OpposingTeamName { get; set; }

        [SerializedName("invalid")]
        public Boolean Invalid { get; set; }

        [SerializedName("deaths")]
        public Int32 Deaths { get; set; }

        [SerializedName("gameId")]
        public Double GameId { get; set; }

        [SerializedName("kills")]
        public Int32 Kills { get; set; }

        [SerializedName("win")]
        public Boolean Win { get; set; }

        [SerializedName("date")]
        public Double Date { get; set; }

        [SerializedName("opposingTeamKills")]
        public Int32 OpposingTeamKills { get; set; }
    }
}
