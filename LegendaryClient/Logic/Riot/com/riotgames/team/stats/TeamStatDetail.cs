using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Team
{
    [Serializable]
    [SerializedName("com.riotgames.team.stats.TeamStatDetail")]
    public class TeamStatDetail
    {
        [SerializedName("maxRating")]
        public int MaxRating { get; set; }

        [SerializedName("teamIdString")]
        public String TeamIdString { get; set; }

        [SerializedName("seedRating")]
        public int SeedRating { get; set; }

        [SerializedName("losses")]
        public int Losses { get; set; }

        [SerializedName("rating")]
        public int Rating { get; set; }

        [SerializedName("teamStatTypeString")]
        public String TeamStatTypeString { get; set; }

        [SerializedName("averageGamesPlayed")]
        public int AverageGamesPlayed { get; set; }

        [SerializedName("teamId")]
        public TeamId TeamId { get; set; }

        [SerializedName("wins")]
        public int Wins { get; set; }

        [SerializedName("teamStatType")]
        public String TeamStatType { get; set; }
    }
}
