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
        public string GameMode { get; set; }

        [InternalName("mapId")]
        public int MapId { get; set; }

        [InternalName("assists")]
        public int Assists { get; set; }

        [InternalName("opposingTeamName")]
        public string OpposingTeamName { get; set; }

        [InternalName("invalid")]
        public bool Invalid { get; set; }

        [InternalName("deaths")]
        public int Deaths { get; set; }

        [InternalName("gameId")]
        public double GameId { get; set; }

        [InternalName("kills")]
        public int Kills { get; set; }

        [InternalName("win")]
        public bool Win { get; set; }

        [InternalName("date")]
        public double Date { get; set; }

        [InternalName("opposingTeamKills")]
        public int OpposingTeamKills { get; set; }
    }
}