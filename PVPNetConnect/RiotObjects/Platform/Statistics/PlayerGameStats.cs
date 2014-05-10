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
        public Boolean Ranked { get; set; }

        [InternalName("skinIndex")]
        public Int32 SkinIndex { get; set; }

        [InternalName("fellowPlayers")]
        public List<FellowPlayerInfo> FellowPlayers { get; set; }

        [InternalName("gameType")]
        public String GameType { get; set; }

        [InternalName("experienceEarned")]
        public Double ExperienceEarned { get; set; }

        [InternalName("rawStatsJson")]
        public object RawStatsJson { get; set; }

        [InternalName("eligibleFirstWinOfDay")]
        public Boolean EligibleFirstWinOfDay { get; set; }

        [InternalName("difficulty")]
        public object Difficulty { get; set; }

        [InternalName("gameMapId")]
        public Int32 GameMapId { get; set; }

        [InternalName("leaver")]
        public Boolean Leaver { get; set; }

        [InternalName("spell1")]
        public Double Spell1 { get; set; }

        [InternalName("gameTypeEnum")]
        public String GameTypeEnum { get; set; }

        [InternalName("teamId")]
        public Double TeamId { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }

        [InternalName("statistics")]
        public List<RawStat> Statistics { get; set; }

        [InternalName("spell2")]
        public Double Spell2 { get; set; }

        [InternalName("afk")]
        public Boolean Afk { get; set; }

        [InternalName("id")]
        public object Id { get; set; }

        [InternalName("boostXpEarned")]
        public Double BoostXpEarned { get; set; }

        [InternalName("level")]
        public Double Level { get; set; }

        [InternalName("invalid")]
        public Boolean Invalid { get; set; }

        [InternalName("userId")]
        public Double UserId { get; set; }

        [InternalName("createDate")]
        public DateTime CreateDate { get; set; }

        [InternalName("userServerPing")]
        public Int32 UserServerPing { get; set; }

        [InternalName("adjustedRating")]
        public Int32 AdjustedRating { get; set; }

        [InternalName("premadeSize")]
        public Int32 PremadeSize { get; set; }

        [InternalName("boostIpEarned")]
        public Double BoostIpEarned { get; set; }

        [InternalName("gameId")]
        public Double GameId { get; set; }

        [InternalName("timeInQueue")]
        public Int32 TimeInQueue { get; set; }

        [InternalName("ipEarned")]
        public Double IpEarned { get; set; }

        [InternalName("eloChange")]
        public Int32 EloChange { get; set; }

        [InternalName("gameMode")]
        public String GameMode { get; set; }

        [InternalName("difficultyString")]
        public object DifficultyString { get; set; }

        [InternalName("KCoefficient")]
        public Double KCoefficient { get; set; }

        [InternalName("teamRating")]
        public Int32 TeamRating { get; set; }

        [InternalName("subType")]
        public String SubType { get; set; }

        [InternalName("queueType")]
        public String QueueType { get; set; }

        [InternalName("premadeTeam")]
        public Boolean PremadeTeam { get; set; }

        [InternalName("predictedWinPct")]
        public Double PredictedWinPct { get; set; }

        [InternalName("rating")]
        public Double Rating { get; set; }

        [InternalName("championId")]
        public Double ChampionId { get; set; }
    }
}