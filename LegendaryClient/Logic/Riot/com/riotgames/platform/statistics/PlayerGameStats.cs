using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.PlayerGameStats")]
    public class PlayerGameStats
    {
        [SerializedName("skinName")]
        public object SkinName { get; set; }

        [SerializedName("ranked")]
        public Boolean Ranked { get; set; }

        [SerializedName("skinIndex")]
        public Int32 SkinIndex { get; set; }

        [SerializedName("fellowPlayers")]
        public List<FellowPlayerInfo> FellowPlayers { get; set; }

        [SerializedName("gameType")]
        public String GameType { get; set; }

        [SerializedName("experienceEarned")]
        public Double ExperienceEarned { get; set; }

        [SerializedName("rawStatsJson")]
        public object RawStatsJson { get; set; }

        [SerializedName("eligibleFirstWinOfDay")]
        public Boolean EligibleFirstWinOfDay { get; set; }

        [SerializedName("difficulty")]
        public object Difficulty { get; set; }

        [SerializedName("gameMapId")]
        public Int32 GameMapId { get; set; }

        [SerializedName("leaver")]
        public Boolean Leaver { get; set; }

        [SerializedName("spell1")]
        public Double Spell1 { get; set; }

        [SerializedName("gameTypeEnum")]
        public String GameTypeEnum { get; set; }

        [SerializedName("teamId")]
        public Double TeamId { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }

        [SerializedName("statistics")]
        public List<RawStat> Statistics { get; set; }

        [SerializedName("spell2")]
        public Double Spell2 { get; set; }

        [SerializedName("afk")]
        public Boolean Afk { get; set; }

        [SerializedName("id")]
        public object Id { get; set; }

        [SerializedName("boostXpEarned")]
        public Double BoostXpEarned { get; set; }

        [SerializedName("level")]
        public Double Level { get; set; }

        [SerializedName("invalid")]
        public Boolean Invalid { get; set; }

        [SerializedName("userId")]
        public Double UserId { get; set; }

        [SerializedName("createDate")]
        public DateTime CreateDate { get; set; }

        [SerializedName("userServerPing")]
        public Int32 UserServerPing { get; set; }

        [SerializedName("adjustedRating")]
        public Int32 AdjustedRating { get; set; }

        [SerializedName("premadeSize")]
        public Int32 PremadeSize { get; set; }

        [SerializedName("boostIpEarned")]
        public Double BoostIpEarned { get; set; }

        [SerializedName("gameId")]
        public Double GameId { get; set; }

        [SerializedName("timeInQueue")]
        public Int32 TimeInQueue { get; set; }

        [SerializedName("ipEarned")]
        public Double IpEarned { get; set; }

        [SerializedName("eloChange")]
        public Int32 EloChange { get; set; }

        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("difficultyString")]
        public object DifficultyString { get; set; }

        [SerializedName("KCoefficient")]
        public Double KCoefficient { get; set; }

        [SerializedName("teamRating")]
        public Int32 TeamRating { get; set; }

        [SerializedName("subType")]
        public String SubType { get; set; }

        [SerializedName("queueType")]
        public String QueueType { get; set; }

        [SerializedName("premadeTeam")]
        public Boolean PremadeTeam { get; set; }

        [SerializedName("predictedWinPct")]
        public Double PredictedWinPct { get; set; }

        [SerializedName("rating")]
        public Double Rating { get; set; }

        [SerializedName("championId")]
        public Double ChampionId { get; set; }
    }
}
