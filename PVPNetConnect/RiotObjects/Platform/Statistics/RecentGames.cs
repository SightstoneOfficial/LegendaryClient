using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class RecentGames : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.RecentGames";

        public RecentGames()
        {
        }

        public RecentGames(Callback callback)
        {
            this.callback = callback;
        }

        public RecentGames(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(RecentGames result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("recentGamesJson")]
        public object RecentGamesJson { get; set; }

        [InternalName("playerGameStatsMap")]
        public TypedObject PlayerGameStatsMap { get; set; }

        [InternalName("gameStatistics")]
        public List<PlayerGameStats> GameStatistics { get; set; }

        [InternalName("userId")]
        public Double UserId { get; set; }
    }
}