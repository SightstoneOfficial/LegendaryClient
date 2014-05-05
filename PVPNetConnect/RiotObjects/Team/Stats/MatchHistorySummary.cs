using System;

namespace PVPNetConnect.RiotObjects.Team.Stats
{
    public class MatchHistorySummary : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.team.stats.MatchHistorySummary";

        public MatchHistorySummary()
        {
        }

        public MatchHistorySummary(Callback callback)
        {
            this.callback = callback;
        }

        public MatchHistorySummary(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(MatchHistorySummary result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("gameMode")]
        public String GameMode { get; set; }

        [InternalName("mapId")]
        public Int32 MapId { get; set; }

        [InternalName("assists")]
        public Int32 Assists { get; set; }

        [InternalName("opposingTeamName")]
        public String OpposingTeamName { get; set; }

        [InternalName("invalid")]
        public Boolean Invalid { get; set; }

        [InternalName("deaths")]
        public Int32 Deaths { get; set; }

        [InternalName("gameId")]
        public Double GameId { get; set; }

        [InternalName("kills")]
        public Int32 Kills { get; set; }

        [InternalName("win")]
        public Boolean Win { get; set; }

        [InternalName("date")]
        public Double Date { get; set; }

        [InternalName("opposingTeamKills")]
        public Int32 OpposingTeamKills { get; set; }
    }
}