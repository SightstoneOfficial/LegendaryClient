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
        public Int32 TalentPointsGained { get; set; }

        [SerializedName("ranked")]
        public Boolean Ranked { get; set; }

        [SerializedName("leveledUp")]
        public Boolean LeveledUp { get; set; }

        [SerializedName("skinIndex")]
        public Int32 SkinIndex { get; set; }

        [SerializedName("queueBonusEarned")]
        public Int32 QueueBonusEarned { get; set; }

        [SerializedName("gameType")]
        public String GameType { get; set; }

        [SerializedName("experienceEarned")]
        public Double ExperienceEarned { get; set; }

        [SerializedName("imbalancedTeamsNoPoints")]
        public Boolean ImbalancedTeamsNoPoints { get; set; }

        [SerializedName("teamPlayerParticipantStats")]
        public List<PlayerParticipantStatsSummary> TeamPlayerParticipantStats { get; set; }

        [SerializedName("basePoints")]
        public Int32 BasePoints { get; set; }

        [SerializedName("reportGameId")]
        public Int32 ReportGameId { get; set; }

        [SerializedName("difficulty")]
        public String Difficulty { get; set; }

        [SerializedName("gameLength")]
        public Double GameLength { get; set; }

        [SerializedName("boostXpEarned")]
        public Double BoostXpEarned { get; set; }

        [SerializedName("invalid")]
        public Boolean Invalid { get; set; }

        [SerializedName("otherTeamInfo")]
        public TeamInfo OtherTeamInfo { get; set; }

        [SerializedName("roomName")]
        public String RoomName { get; set; }

        [SerializedName("customMinutesLeftToday")]
        public Int32 CustomMinutesLeftToday { get; set; }

        [SerializedName("userId")]
        public Int32 UserId { get; set; }

        [SerializedName("pointsPenalties")]
        public List<object> PointsPenalties { get; set; }

        [SerializedName("coOpVsAiMinutesLeftToday")]
        public Int32 CoOpVsAiMinutesLeftToday { get; set; }

        [SerializedName("otherTeamPlayerParticipantStats")]
        public List<PlayerParticipantStatsSummary> OtherTeamPlayerParticipantStats { get; set; }

        [SerializedName("loyaltyBoostIpEarned")]
        public Double LoyaltyBoostIpEarned { get; set; }

        [SerializedName("rpEarned")]
        public Double RpEarned { get; set; }

        [SerializedName("completionBonusPoints")]
        public Int32 CompletionBonusPoints { get; set; }

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
        public String RoomPassword { get; set; }

        [SerializedName("elo")]
        public Int32 Elo { get; set; }

        [SerializedName("ipEarned")]
        public Double IpEarned { get; set; }

        [SerializedName("firstWinBonus")]
        public Double FirstWinBonus { get; set; }

        [SerializedName("sendStatsToTournamentProvider")]
        public Boolean SendStatsToTournamentProvider { get; set; }

        [SerializedName("eloChange")]
        public Int32 EloChange { get; set; }

        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("myTeamInfo")]
        public TeamInfo MyTeamInfo { get; set; }

        [SerializedName("queueType")]
        public String QueueType { get; set; }

        [SerializedName("odinBonusIp")]
        public Int32 OdinBonusIp { get; set; }

        [SerializedName("myTeamStatus")]
        public String MyTeamStatus { get; set; }

        [SerializedName("ipTotal")]
        public Double IpTotal { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("customMsecsUntilReset")]
        public Double CustomMsecsUntilReset { get; set; }

        [SerializedName("rerollEarned")]
        public Double RerollPointsEarned { get; set; }
    }
}