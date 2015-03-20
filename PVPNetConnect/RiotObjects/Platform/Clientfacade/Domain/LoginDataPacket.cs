using PVPNetConnect.RiotObjects.Kudos.Dto;
using PVPNetConnect.RiotObjects.Platform.Broadcast;
using PVPNetConnect.RiotObjects.Platform.Game;
using PVPNetConnect.RiotObjects.Platform.Statistics;
using PVPNetConnect.RiotObjects.Platform.Summoner;
using PVPNetConnect.RiotObjects.Platform.Systemstate;
using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Clientfacade.Domain
{
    public class LoginDataPacket : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.clientfacade.domain.LoginDataPacket";

        public LoginDataPacket()
        {
        }

        public LoginDataPacket(Callback callback)
        {
            this.callback = callback;
        }

        public LoginDataPacket(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(LoginDataPacket result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("restrictedGamesRemainingForRanked")]
        public int restrictedGamesRemainingForRanked { get; set; }

        [InternalName("playerStatSummaries")]
        public PlayerStatSummaries PlayerStatSummaries { get; set; }

        [InternalName("restrictedChatGamesRemaining")]
        public int RestrictedChatGamesRemaining { get; set; }

        [InternalName("minutesUntilShutdown")]
        public int MinutesUntilShutdown { get; set; }

        [InternalName("minor")]
        public Boolean Minor { get; set; }

        [InternalName("maxPracticeGameSize")]
        public int MaxPracticeGameSize { get; set; }

        [InternalName("summonerCatalog")]
        public SummonerCatalog SummonerCatalog { get; set; }

        [InternalName("ipBalance")]
        public Double IpBalance { get; set; }

        [InternalName("reconnectInfo")]
        public PlatformGameLifecycleDTO ReconnectInfo { get; set; }

        [InternalName("languages")]
        public List<string> Languages { get; set; }

        [InternalName("simpleMessages")]
        public List<object> SimpleMessages { get; set; }

        [InternalName("allSummonerData")]
        public AllSummonerData AllSummonerData { get; set; }

        [InternalName("customMinutesLeftToday")]
        public int CustomMinutesLeftToday { get; set; }

        [InternalName("platformGameLifecycleDTO")]
        public object PlatformGameLifecycleDTO { get; set; }

        [InternalName("coOpVsAiMinutesLeftToday")]
        public int CoOpVsAiMinutesLeftToday { get; set; }

        [InternalName("bingeData")]
        public object BingeData { get; set; }

        [InternalName("inGhostGame")]
        public Boolean InGhostGame { get; set; }

        [InternalName("leaverPenaltyLevel")]
        public int LeaverPenaltyLevel { get; set; }

        [InternalName("bingePreventionSystemEnabledForClient")]
        public Boolean BingePreventionSystemEnabledForClient { get; set; }

        [InternalName("pendingBadges")]
        public int PendingBadges { get; set; }

        [InternalName("broadcastNotification")]
        public BroadcastNotification BroadcastNotification { get; set; }

        [InternalName("minutesUntilMidnight")]
        public int MinutesUntilMidnight { get; set; }

        [InternalName("timeUntilFirstWinOfDay")]
        public Double TimeUntilFirstWinOfDay { get; set; }

        [InternalName("coOpVsAiMsecsUntilReset")]
        public Double CoOpVsAiMsecsUntilReset { get; set; }

        [InternalName("clientSystemStates")]
        public ClientSystemStatesNotification ClientSystemStates { get; set; }

        [InternalName("bingeMinutesRemaining")]
        public Double BingeMinutesRemaining { get; set; }

        [InternalName("pendingKudosDTO")]
        public PendingKudosDTO PendingKudosDTO { get; set; }

        [InternalName("leaverBusterPenaltyTime")]
        public int LeaverBusterPenaltyTime { get; set; }

        [InternalName("platformId")]
        public string PlatformId { get; set; }

        [InternalName("matchMakingEnabled")]
        public Boolean MatchMakingEnabled { get; set; }

        [InternalName("minutesUntilShutdownEnabled")]
        public Boolean MinutesUntilShutdownEnabled { get; set; }

        [InternalName("rpBalance")]
        public Double RpBalance { get; set; }

        [InternalName("gameTypeConfigs")]
        public List<GameTypeConfigDTO> GameTypeConfigs { get; set; }

        [InternalName("bingeIsPlayerInBingePreventionWindow")]
        public Boolean BingeIsPlayerInBingePreventionWindow { get; set; }

        [InternalName("minorShutdownEnforced")]
        public Boolean MinorShutdownEnforced { get; set; }

        [InternalName("competitiveRegion")]
        public string CompetitiveRegion { get; set; }

        [InternalName("customMsecsUntilReset")]
        public Double CustomMsecsUntilReset { get; set; }
    }
}