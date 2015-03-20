#region

using System;
using System.Collections.Generic;
using RtmpSharp.IO;

#endregion

namespace LegendaryClient.Logic.Replays
{
    [Serializable]
    [SerializedName("com.riotgames.platform.observer.domain.EndOfGameStats")]
    public class EndOfGameStats
    {
        [SerializedName("talentPointsGained")]
        public int TalentPointsGained { get; set; }

        [SerializedName("ranked")]
        public Boolean Ranked { get; set; }

        [SerializedName("leveledUp")]
        public Boolean LeveledUp { get; set; }

        [SerializedName("skinIndex")]
        public int SkinIndex { get; set; }

        [SerializedName("queueBonusEarned")]
        public int QueueBonusEarned { get; set; }

        [SerializedName("gameType")]
        public string GameType { get; set; }

        [SerializedName("experienceEarned")]
        public Double ExperienceEarned { get; set; }

        [SerializedName("imbalancedTeamsNoPoints")]
        public Boolean ImbalancedTeamsNoPoints { get; set; }

        [SerializedName("teamPlayerParticipantStats")]
        public List<PlayerParticipantStatsSummary> TeamPlayerParticipantStats { get; set; }

        [SerializedName("basePoints")]
        public int BasePoints { get; set; }

        [SerializedName("reportGameId")]
        public int ReportGameId { get; set; }

        [SerializedName("difficulty")]
        public string Difficulty { get; set; }

        [SerializedName("gameLength")]
        public Double GameLength { get; set; }

        [SerializedName("boostXpEarned")]
        public Double BoostXpEarned { get; set; }

        [SerializedName("invalid")]
        public Boolean Invalid { get; set; }

        [SerializedName("otherTeamInfo")]
        public TeamInfo OtherTeamInfo { get; set; }

        [SerializedName("roomName")]
        public string RoomName { get; set; }

        [SerializedName("customMinutesLeftToday")]
        public int CustomMinutesLeftToday { get; set; }

        [SerializedName("userId")]
        public int UserId { get; set; }

        [SerializedName("pointsPenalties")]
        public List<object> PointsPenalties { get; set; }

        [SerializedName("coOpVsAiMinutesLeftToday")]
        public int CoOpVsAiMinutesLeftToday { get; set; }

        [SerializedName("otherTeamPlayerParticipantStats")]
        public List<PlayerParticipantStatsSummary> OtherTeamPlayerParticipantStats { get; set; }

        [SerializedName("loyaltyBoostIpEarned")]
        public Double LoyaltyBoostIpEarned { get; set; }

        [SerializedName("rpEarned")]
        public Double RpEarned { get; set; }

        [SerializedName("completionBonusPoints")]
        public int CompletionBonusPoints { get; set; }

        [SerializedName("coOpVsAiMsecsUntilReset")]
        public Double CoOpVsAiMsecsUntilReset { get; set; }

        [SerializedName("boostIpEarned")]
        public Double BoostIpEarned { get; set; }

        [SerializedName("newSpells")]
        public List<object> NewSpells { get; set; }

        [SerializedName("experienceTotal")]
        public Double ExperienceTotal { get; set; }

        [SerializedName("gameId")]
        public Double GameId { get; set; }

        [SerializedName("timeUntilNextFirstWinBonus")]
        public Double TimeUntilNextFirstWinBonus { get; set; }

        [SerializedName("loyaltyBoostXpEarned")]
        public Double LoyaltyBoostXpEarned { get; set; }

        [SerializedName("roomPassword")]
        public string RoomPassword { get; set; }

        [SerializedName("elo")]
        public int Elo { get; set; }

        [SerializedName("ipEarned")]
        public Double IpEarned { get; set; }

        [SerializedName("firstWinBonus")]
        public Double FirstWinBonus { get; set; }

        [SerializedName("sendStatsToTournamentProvider")]
        public Boolean SendStatsToTournamentProvider { get; set; }

        [SerializedName("eloChange")]
        public int EloChange { get; set; }

        [SerializedName("gameMode")]
        public string GameMode { get; set; }

        [SerializedName("myTeamInfo")]
        public TeamInfo MyTeamInfo { get; set; }

        [SerializedName("queueType")]
        public string QueueType { get; set; }

        [SerializedName("odinBonusIp")]
        public int OdinBonusIp { get; set; }

        [SerializedName("myTeamStatus")]
        public string MyTeamStatus { get; set; }

        [SerializedName("ipTotal")]
        public Double IpTotal { get; set; }

        [SerializedName("summonerName")]
        public string SummonerName { get; set; }

        [SerializedName("customMsecsUntilReset")]
        public Double CustomMsecsUntilReset { get; set; }

        [SerializedName("rerollEarned")]
        public Double RerollPointsEarned { get; set; }
    }
}