using System;
using System.Collections.Generic;
using Sightstone.Logic.Riot.Team;
using RtmpSharp.IO;

namespace Sightstone.Logic.Replays
{
    [Serializable]
    [SerializedName("com.riotgames.platform.observer.domain.EndOfGameStats")]
    class EndOfReplayGameStats
    {
        [SerializedName("talentPointsGained")]
        public int TalentPointsGained { get; set; }

        [SerializedName("ranked")]
        public bool Ranked { get; set; }

        [SerializedName("leveledUp")]
        public bool LeveledUp { get; set; }

        [SerializedName("skinIndex")]
        public int SkinIndex { get; set; }

        [SerializedName("queueBonusEarned")]
        public int QueueBonusEarned { get; set; }

        [SerializedName("gameType")]
        public string GameType { get; set; }

        [SerializedName("experienceEarned")]
        public double ExperienceEarned { get; set; }

        [SerializedName("imbalancedTeamsNoPoints")]
        public bool ImbalancedTeamsNoPoints { get; set; }

        [SerializedName("teamPlayerParticipantStats")]
        public List<ReplayParticipantStatsSummary> TeamPlayerParticipantStats { get; set; }

        [SerializedName("basePoints")]
        public int BasePoints { get; set; }

        [SerializedName("reportGameId")]
        public int ReportGameId { get; set; }

        [SerializedName("difficulty")]
        public string Difficulty { get; set; }

        [SerializedName("gameLength")]
        public double GameLength { get; set; }

        [SerializedName("boostXpEarned")]
        public double BoostXpEarned { get; set; }

        [SerializedName("invalid")]
        public bool Invalid { get; set; }

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
        public List<ReplayParticipantStatsSummary> OtherTeamPlayerParticipantStats { get; set; }

        [SerializedName("loyaltyBoostIpEarned")]
        public double LoyaltyBoostIpEarned { get; set; }

        [SerializedName("rpEarned")]
        public double RpEarned { get; set; }

        [SerializedName("completionBonusPoints")]
        public int CompletionBonusPoints { get; set; }

        [SerializedName("coOpVsAiMsecsUntilReset")]
        public double CoOpVsAiMsecsUntilReset { get; set; }

        [SerializedName("boostIpEarned")]
        public double BoostIpEarned { get; set; }

        [SerializedName("newSpells")]
        public List<object> NewSpells { get; set; }

        [SerializedName("experienceTotal")]
        public double ExperienceTotal { get; set; }

        [SerializedName("gameId")]
        public double GameId { get; set; }

        [SerializedName("timeUntilNextFirstWinBonus")]
        public double TimeUntilNextFirstWinBonus { get; set; }

        [SerializedName("loyaltyBoostXpEarned")]
        public double LoyaltyBoostXpEarned { get; set; }

        [SerializedName("roomPassword")]
        public string RoomPassword { get; set; }

        [SerializedName("elo")]
        public int Elo { get; set; }

        [SerializedName("ipEarned")]
        public double IpEarned { get; set; }

        [SerializedName("firstWinBonus")]
        public double FirstWinBonus { get; set; }

        [SerializedName("sendStatsToTournamentProvider")]
        public bool SendStatsToTournamentProvider { get; set; }

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
        public double IpTotal { get; set; }

        [SerializedName("summonerName")]
        public string SummonerName { get; set; }

        [SerializedName("customMsecsUntilReset")]
        public double CustomMsecsUntilReset { get; set; }

        [SerializedName("rerollEarned")]
        public double RerollPointsEarned { get; set; }
    }
}
