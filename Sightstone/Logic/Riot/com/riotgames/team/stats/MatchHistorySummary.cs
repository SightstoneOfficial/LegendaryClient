using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Team
{
    [Serializable]
    [SerializedName("com.riotgames.team.stats.MatchHistorySummary")]
    public class MatchHistorySummary
    {
        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("mapId")]
        public int MapId { get; set; }

        [SerializedName("assists")]
        public int Assists { get; set; }

        [SerializedName("opposingTeamName")]
        public String OpposingTeamName { get; set; }

        [SerializedName("invalid")]
        public bool Invalid { get; set; }

        [SerializedName("deaths")]
        public int Deaths { get; set; }

        [SerializedName("gameId")]
        public double GameId { get; set; }

        [SerializedName("kills")]
        public int Kills { get; set; }

        [SerializedName("win")]
        public bool Win { get; set; }

        [SerializedName("date")]
        public double Date { get; set; }

        [SerializedName("opposingTeamKills")]
        public int OpposingTeamKills { get; set; }
    }
}
