using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Leagues
{
    [Serializable]
    [SerializedName("com.riotgames.leagues.pojo.LeagueItemDTO")]
    public class LeagueItemDTO
    {
        [SerializedName("previousDayLeaguePosition")]
        public int PreviousDayLeaguePosition { get; set; }

        [SerializedName("timeLastDecayMessageShown")]
        public double TimeLastDecayMessageShown { get; set; }

        [SerializedName("hotStreak")]
        public bool HotStreak { get; set; }

        [SerializedName("leagueName")]
        public String LeagueName { get; set; }

        [SerializedName("miniSeries")]
        public MiniSeriesDTO MiniSeries { get; set; }

        [SerializedName("tier")]
        public String Tier { get; set; }

        [SerializedName("freshBlood")]
        public bool FreshBlood { get; set; }

        [SerializedName("lastPlayed")]
        public double LastPlayed { get; set; }

        [SerializedName("playerOrTeamId")]
        public String PlayerOrTeamId { get; set; }

        [SerializedName("leaguePoints")]
        public int LeaguePoints { get; set; }

        [SerializedName("inactive")]
        public bool Inactive { get; set; }

        [SerializedName("rank")]
        public String Rank { get; set; }

        [SerializedName("veteran")]
        public bool Veteran { get; set; }

        [SerializedName("queueType")]
        public String QueueType { get; set; }

        [SerializedName("losses")]
        public int Losses { get; set; }

        [SerializedName("timeUntilDecay")]
        public double TimeUntilDecay { get; set; }

        [SerializedName("playerOrTeamName")]
        public String PlayerOrTeamName { get; set; }

        [SerializedName("wins")]
        public int Wins { get; set; }
    }
}
