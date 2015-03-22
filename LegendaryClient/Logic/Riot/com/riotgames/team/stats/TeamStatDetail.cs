using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Team
{
    [Serializable]
    [SerializedName("com.riotgames.team.stats.TeamStatDetail")]
    public class TeamStatDetail
    {
        [SerializedName("maxRating")]
        public Int32 MaxRating { get; set; }

        [SerializedName("teamIdString")]
        public String TeamIdString { get; set; }

        [SerializedName("seedRating")]
        public Int32 SeedRating { get; set; }

        [SerializedName("losses")]
        public Int32 Losses { get; set; }

        [SerializedName("rating")]
        public Int32 Rating { get; set; }

        [SerializedName("teamStatTypeString")]
        public String TeamStatTypeString { get; set; }

        [SerializedName("averageGamesPlayed")]
        public Int32 AverageGamesPlayed { get; set; }

        [SerializedName("teamId")]
        public TeamId TeamId { get; set; }

        [SerializedName("wins")]
        public Int32 Wins { get; set; }

        [SerializedName("teamStatType")]
        public String TeamStatType { get; set; }
    }
}
