using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class PlayerGameStats : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.PlayerGameStats";

        public PlayerGameStats()
        {
        }

        public PlayerGameStats(Callback callback)
        {
            this.callback = callback;
        }

        public PlayerGameStats(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PlayerGameStats result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("skinName")]
        public object SkinName { get; set; }

        [InternalName("ranked")]
        public bool Ranked { get; set; }

        [InternalName("skinIndex")]
        public int SkinIndex { get; set; }

        [InternalName("fellowPlayers")]
        public List<FellowPlayerInfo> FellowPlayers { get; set; }

        [InternalName("gameType")]
        public string GameType { get; set; }

        [InternalName("experienceEarned")]
        public double ExperienceEarned { get; set; }

        [InternalName("rawStatsJson")]
        public object RawStatsJson { get; set; }

        [InternalName("eligibleFirstWinOfDay")]
        public bool EligibleFirstWinOfDay { get; set; }

        [InternalName("difficulty")]
        public object Difficulty { get; set; }

        [InternalName("gameMapId")]
        public int GameMapId { get; set; }

        [InternalName("leaver")]
        public bool Leaver { get; set; }

        [InternalName("spell1")]
        public double Spell1 { get; set; }

        [InternalName("gameTypeEnum")]
        public string GameTypeEnum { get; set; }

        [InternalName("teamId")]
        public double TeamId { get; set; }

        [InternalName("summonerId")]
        public double SummonerId { get; set; }

        [InternalName("statistics")]
        public List<RawStat> Statistics { get; set; }

        [InternalName("spell2")]
        public double Spell2 { get; set; }

        [InternalName("afk")]
        public bool Afk { get; set; }

        [InternalName("id")]
        public object Id { get; set; }

        [InternalName("boostXpEarned")]
        public double BoostXpEarned { get; set; }

        [InternalName("level")]
        public double Level { get; set; }

        [InternalName("invalid")]
        public bool Invalid { get; set; }

        [InternalName("userId")]
        public double UserId { get; set; }

        [InternalName("createDate")]
        public DateTime CreateDate { get; set; }

        [InternalName("userServerPing")]
        public int UserServerPing { get; set; }

        [InternalName("adjustedRating")]
        public int AdjustedRating { get; set; }

        [InternalName("premadeSize")]
        public int PremadeSize { get; set; }

        [InternalName("boostIpEarned")]
        public double BoostIpEarned { get; set; }

        [InternalName("gameId")]
        public double GameId { get; set; }

        [InternalName("timeInQueue")]
        public int TimeInQueue { get; set; }

        [InternalName("ipEarned")]
        public double IpEarned { get; set; }

        [InternalName("eloChange")]
        public int EloChange { get; set; }

        [InternalName("gameMode")]
        public string GameMode { get; set; }

        [InternalName("difficultyString")]
        public object DifficultyString { get; set; }

        [InternalName("KCoefficient")]
        public double KCoefficient { get; set; }

        [InternalName("teamRating")]
        public int TeamRating { get; set; }

        [InternalName("subType")]
        public string SubType { get; set; }

        [InternalName("queueType")]
        public string QueueType { get; set; }

        [InternalName("premadeTeam")]
        public bool PremadeTeam { get; set; }

        [InternalName("predictedWinPct")]
        public double PredictedWinPct { get; set; }

        [InternalName("rating")]
        public double Rating { get; set; }

        [InternalName("championId")]
        public double ChampionId { get; set; }
    }
}