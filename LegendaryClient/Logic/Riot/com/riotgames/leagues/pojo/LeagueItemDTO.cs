using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Leagues
{
    [Serializable]
    [SerializedName("com.riotgames.leagues.pojo.LeagueItemDTO")]
    public class LeagueItemDTO
    {
        [SerializedName("previousDayLeaguePosition")]
        public Int32 PreviousDayLeaguePosition { get; set; }

        [SerializedName("timeLastDecayMessageShown")]
        public Double TimeLastDecayMessageShown { get; set; }

        [SerializedName("hotStreak")]
        public Boolean HotStreak { get; set; }

        [SerializedName("leagueName")]
        public String LeagueName { get; set; }

        [SerializedName("miniSeries")]
        public MiniSeriesDTO MiniSeries { get; set; }

        [SerializedName("tier")]
        public String Tier { get; set; }

        [SerializedName("freshBlood")]
        public Boolean FreshBlood { get; set; }

        [SerializedName("lastPlayed")]
        public Double LastPlayed { get; set; }

        [SerializedName("playerOrTeamId")]
        public String PlayerOrTeamId { get; set; }

        [SerializedName("leaguePoints")]
        public Int32 LeaguePoints { get; set; }

        [SerializedName("inactive")]
        public Boolean Inactive { get; set; }

        [SerializedName("rank")]
        public String Rank { get; set; }

        [SerializedName("veteran")]
        public Boolean Veteran { get; set; }

        [SerializedName("queueType")]
        public String QueueType { get; set; }

        [SerializedName("losses")]
        public Int32 Losses { get; set; }

        [SerializedName("timeUntilDecay")]
        public Double TimeUntilDecay { get; set; }

        [SerializedName("playerOrTeamName")]
        public String PlayerOrTeamName { get; set; }

        [SerializedName("wins")]
        public Int32 Wins { get; set; }
    }
}
