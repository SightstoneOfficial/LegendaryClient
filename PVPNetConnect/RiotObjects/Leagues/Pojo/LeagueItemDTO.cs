using System;

namespace PVPNetConnect.RiotObjects.Leagues.Pojo
{
    public class LeagueItemDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.leagues.pojo.LeagueItemDTO";

        public LeagueItemDTO()
        {
        }

        public LeagueItemDTO(Callback callback)
        {
            this.callback = callback;
        }

        public LeagueItemDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(LeagueItemDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("previousDayLeaguePosition")]
        public int PreviousDayLeaguePosition { get; set; }

        [InternalName("timeLastDecayMessageShown")]
        public Double TimeLastDecayMessageShown { get; set; }

        [InternalName("hotStreak")]
        public Boolean HotStreak { get; set; }

        [InternalName("leagueName")]
        public string LeagueName { get; set; }

        [InternalName("miniSeries")]
        public object MiniSeries { get; set; }

        [InternalName("tier")]
        public string Tier { get; set; }

        [InternalName("freshBlood")]
        public Boolean FreshBlood { get; set; }

        [InternalName("lastPlayed")]
        public Double LastPlayed { get; set; }

        [InternalName("playerOrTeamId")]
        public string PlayerOrTeamId { get; set; }

        [InternalName("leaguePoints")]
        public int LeaguePoints { get; set; }

        [InternalName("inactive")]
        public Boolean Inactive { get; set; }

        [InternalName("rank")]
        public string Rank { get; set; }

        [InternalName("veteran")]
        public Boolean Veteran { get; set; }

        [InternalName("queueType")]
        public string QueueType { get; set; }

        [InternalName("losses")]
        public int Losses { get; set; }

        [InternalName("timeUntilDecay")]
        public Double TimeUntilDecay { get; set; }

        [InternalName("playerOrTeamName")]
        public string PlayerOrTeamName { get; set; }

        [InternalName("wins")]
        public int Wins { get; set; }
    }
}