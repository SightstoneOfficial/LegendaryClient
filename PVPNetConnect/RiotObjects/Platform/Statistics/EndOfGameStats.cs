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
        public Int32 TalentPointsGained { get; set; }

        [InternalName("ranked")]
        public Boolean Ranked { get; set; }

        [InternalName("leveledUp")]
        public Boolean LeveledUp { get; set; }

        [InternalName("skinIndex")]
        public Int32 SkinIndex { get; set; }

        [InternalName("queueBonusEarned")]
        public Int32 QueueBonusEarned { get; set; }

        [InternalName("gameType")]
        public String GameType { get; set; }

        [InternalName("experienceEarned")]
        public Double ExperienceEarned { get; set; }

        [InternalName("imbalancedTeamsNoPoints")]
        public Boolean ImbalancedTeamsNoPoints { get; set; }

        [InternalName("teamPlayerParticipantStats")]
        public List<PlayerParticipantStatsSummary> TeamPlayerParticipantStats { get; set; }

        [InternalName("basePoints")]
        public Int32 BasePoints { get; set; }

        [InternalName("reportGameId")]
        public Int32 ReportGameId { get; set; }

        [InternalName("difficulty")]
        public String Difficulty { get; set; }

        [InternalName("gameLength")]
        public Double GameLength { get; set; }

        [InternalName("boostXpEarned")]
        public Double BoostXpEarned { get; set; }

        [InternalName("invalid")]
        public Boolean Invalid { get; set; }

        [InternalName("otherTeamInfo")]
        public TeamInfo OtherTeamInfo { get; set; }

        [InternalName("roomName")]
        public String RoomName { get; set; }

        [InternalName("customMinutesLeftToday")]
        public Int32 CustomMinutesLeftToday { get; set; }

        [InternalName("userId")]
        public Int32 UserId { get; set; }

        [InternalName("pointsPenalties")]
        public List<object> PointsPenalties { get; set; }

        [InternalName("coOpVsAiMinutesLeftToday")]
        public Int32 CoOpVsAiMinutesLeftToday { get; set; }

        [InternalName("otherTeamPlayerParticipantStats")]
        public List<PlayerParticipantStatsSummary> OtherTeamPlayerParticipantStats { get; set; }

        [InternalName("loyaltyBoostIpEarned")]
        public Double LoyaltyBoostIpEarned { get; set; }

        [InternalName("rpEarned")]
        public Double RpEarned { get; set; }

        [InternalName("completionBonusPoints")]
        public Int32 CompletionBonusPoints { get; set; }

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
        public String RoomPassword { get; set; }

        [InternalName("elo")]
        public Int32 Elo { get; set; }

        [InternalName("ipEarned")]
        public Double IpEarned { get; set; }

        [InternalName("firstWinBonus")]
        public Double FirstWinBonus { get; set; }

        [InternalName("sendStatsToTournamentProvider")]
        public Boolean SendStatsToTournamentProvider { get; set; }

        [InternalName("eloChange")]
        public Int32 EloChange { get; set; }

        [InternalName("gameMode")]
        public String GameMode { get; set; }

        [InternalName("myTeamInfo")]
        public TeamInfo MyTeamInfo { get; set; }

        [InternalName("queueType")]
        public String QueueType { get; set; }

        [InternalName("odinBonusIp")]
        public Int32 OdinBonusIp { get; set; }

        [InternalName("myTeamStatus")]
        public String MyTeamStatus { get; set; }

        [InternalName("ipTotal")]
        public Double IpTotal { get; set; }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }

        [InternalName("customMsecsUntilReset")]
        public Double CustomMsecsUntilReset { get; set; }

        [InternalName("rerollEarned")]
        public Double RerollPointsEarned { get; set; }
    }
}