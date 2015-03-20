using PVPNetConnect.RiotObjects.Team;
using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class EndOfGameStats : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.EndOfGameStats";

        public EndOfGameStats()
        {
        }

        public EndOfGameStats(Callback callback)
        {
            this.callback = callback;
        }

        public EndOfGameStats(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(EndOfGameStats result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("talentPointsGained")]
        public int TalentPointsGained { get; set; }

        [InternalName("ranked")]
        public Boolean Ranked { get; set; }

        [InternalName("leveledUp")]
        public Boolean LeveledUp { get; set; }

        [InternalName("skinIndex")]
        public int SkinIndex { get; set; }

        [InternalName("queueBonusEarned")]
        public int QueueBonusEarned { get; set; }

        [InternalName("gameType")]
        public string GameType { get; set; }

        [InternalName("experienceEarned")]
        public Double ExperienceEarned { get; set; }

        [InternalName("imbalancedTeamsNoPoints")]
        public Boolean ImbalancedTeamsNoPoints { get; set; }

        [InternalName("teamPlayerParticipantStats")]
        public List<PlayerParticipantStatsSummary> TeamPlayerParticipantStats { get; set; }

        [InternalName("basePoints")]
        public int BasePoints { get; set; }

        [InternalName("reportGameId")]
        public int ReportGameId { get; set; }

        [InternalName("difficulty")]
        public string Difficulty { get; set; }

        [InternalName("gameLength")]
        public Double GameLength { get; set; }

        [InternalName("boostXpEarned")]
        public Double BoostXpEarned { get; set; }

        [InternalName("invalid")]
        public Boolean Invalid { get; set; }

        [InternalName("otherTeamInfo")]
        public TeamInfo OtherTeamInfo { get; set; }

        [InternalName("roomName")]
        public string RoomName { get; set; }

        [InternalName("customMinutesLeftToday")]
        public int CustomMinutesLeftToday { get; set; }

        [InternalName("userId")]
        public int UserId { get; set; }

        [InternalName("pointsPenalties")]
        public List<object> PointsPenalties { get; set; }

        [InternalName("coOpVsAiMinutesLeftToday")]
        public int CoOpVsAiMinutesLeftToday { get; set; }

        [InternalName("otherTeamPlayerParticipantStats")]
        public List<PlayerParticipantStatsSummary> OtherTeamPlayerParticipantStats { get; set; }

        [InternalName("loyaltyBoostIpEarned")]
        public Double LoyaltyBoostIpEarned { get; set; }

        [InternalName("rpEarned")]
        public Double RpEarned { get; set; }

        [InternalName("completionBonusPoints")]
        public int CompletionBonusPoints { get; set; }

        [InternalName("coOpVsAiMsecsUntilReset")]
        public Double CoOpVsAiMsecsUntilReset { get; set; }

        [InternalName("boostIpEarned")]
        public Double BoostIpEarned { get; set; }

        [InternalName("newSpells")]
        public List<object> NewSpells { get; set; }

        [InternalName("experienceTotal")]
        public Double ExperienceTotal { get; set; }

        [InternalName("gameId")]
        public Double GameId { get; set; }

        [InternalName("timeUntilNextFirstWinBonus")]
        public Double TimeUntilNextFirstWinBonus { get; set; }

        [InternalName("loyaltyBoostXpEarned")]
        public Double LoyaltyBoostXpEarned { get; set; }

        [InternalName("roomPassword")]
        public string RoomPassword { get; set; }

        [InternalName("elo")]
        public int Elo { get; set; }

        [InternalName("ipEarned")]
        public Double IpEarned { get; set; }

        [InternalName("firstWinBonus")]
        public Double FirstWinBonus { get; set; }

        [InternalName("sendStatsToTournamentProvider")]
        public Boolean SendStatsToTournamentProvider { get; set; }

        [InternalName("eloChange")]
        public int EloChange { get; set; }

        [InternalName("gameMode")]
        public string GameMode { get; set; }

        [InternalName("myTeamInfo")]
        public TeamInfo MyTeamInfo { get; set; }

        [InternalName("queueType")]
        public string QueueType { get; set; }

        [InternalName("odinBonusIp")]
        public int OdinBonusIp { get; set; }

        [InternalName("myTeamStatus")]
        public string MyTeamStatus { get; set; }

        [InternalName("ipTotal")]
        public Double IpTotal { get; set; }

        [InternalName("summonerName")]
        public string SummonerName { get; set; }

        [InternalName("customMsecsUntilReset")]
        public Double CustomMsecsUntilReset { get; set; }

        [InternalName("rerollEarned")]
        public Double RerollPointsEarned { get; set; }
    }
}