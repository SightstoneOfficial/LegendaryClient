using System;

namespace PVPNetConnect.RiotObjects.Team.Stats
{
    public class TeamStatDetail : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.team.stats.TeamStatDetail";

        public TeamStatDetail()
        {
        }

        public TeamStatDetail(Callback callback)
        {
            this.callback = callback;
        }

        public TeamStatDetail(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TeamStatDetail result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("maxRating")]
        public int MaxRating { get; set; }

        [InternalName("teamIdString")]
        public string TeamIdString { get; set; }

        [InternalName("seedRating")]
        public int SeedRating { get; set; }

        [InternalName("losses")]
        public int Losses { get; set; }

        [InternalName("rating")]
        public int Rating { get; set; }

        [InternalName("teamStatTypeString")]
        public string TeamStatTypeString { get; set; }

        [InternalName("averageGamesPlayed")]
        public int AverageGamesPlayed { get; set; }

        [InternalName("teamId")]
        public TeamId TeamId { get; set; }

        [InternalName("wins")]
        public int Wins { get; set; }

        [InternalName("teamStatType")]
        public string TeamStatType { get; set; }
    }
}