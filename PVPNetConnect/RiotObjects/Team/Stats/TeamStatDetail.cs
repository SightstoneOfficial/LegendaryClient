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
        public Int32 MaxRating { get; set; }

        [InternalName("teamIdString")]
        public String TeamIdString { get; set; }

        [InternalName("seedRating")]
        public Int32 SeedRating { get; set; }

        [InternalName("losses")]
        public Int32 Losses { get; set; }

        [InternalName("rating")]
        public Int32 Rating { get; set; }

        [InternalName("teamStatTypeString")]
        public String TeamStatTypeString { get; set; }

        [InternalName("averageGamesPlayed")]
        public Int32 AverageGamesPlayed { get; set; }

        [InternalName("teamId")]
        public TeamId TeamId { get; set; }

        [InternalName("wins")]
        public Int32 Wins { get; set; }

        [InternalName("teamStatType")]
        public String TeamStatType { get; set; }
    }
}