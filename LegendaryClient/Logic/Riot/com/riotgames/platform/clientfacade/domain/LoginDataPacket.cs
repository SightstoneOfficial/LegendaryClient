using System;
using RtmpSharp.IO;
using System.Collections.Generic;
using LegendaryClient.Logic.Riot.Kudos;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.clientfacade.domain.LoginDataPacket")]
    public class LoginDataPacket
    {
        [SerializedName("restrictedGamesRemainingForRanked")]
        public int RestrictedGamesRemainingForRanked { get; set; }

        [SerializedName("playerStatSummaries")]
        public PlayerStatSummaries PlayerStatSummaries { get; set; }

        [SerializedName("restrictedChatGamesRemaining")]
        public Int32 RestrictedChatGamesRemaining { get; set; }

        [SerializedName("minutesUntilShutdown")]
        public Int32 MinutesUntilShutdown { get; set; }

        [SerializedName("minor")]
        public Boolean Minor { get; set; }

        [SerializedName("maxPracticeGameSize")]
        public Int32 MaxPracticeGameSize { get; set; }

        [SerializedName("summonerCatalog")]
        public SummonerCatalog SummonerCatalog { get; set; }

        [SerializedName("ipBalance")]
        public Double IpBalance { get; set; }

        [SerializedName("reconnectInfo")]
        public object ReconnectInfo { get; set; }

        [SerializedName("languages")]
        public List<String> Languages { get; set; }

        [SerializedName("simpleMessages")]
        public List<object> SimpleMessages { get; set; }

        [SerializedName("allSummonerData")]
        public AllSummonerData AllSummonerData { get; set; }

        [SerializedName("customMinutesLeftToday")]
        public Int32 CustomMinutesLeftToday { get; set; }

        [SerializedName("platformGameLifecycleDTO")]
        public object PlatformGameLifecycleDTO { get; set; }

        [SerializedName("coOpVsAiMinutesLeftToday")]
        public Int32 CoOpVsAiMinutesLeftToday { get; set; }

        [SerializedName("bingeData")]
        public object BingeData { get; set; }

        [SerializedName("inGhostGame")]
        public Boolean InGhostGame { get; set; }

        [SerializedName("leaverPenaltyLevel")]
        public Int32 LeaverPenaltyLevel { get; set; }

        [SerializedName("bingePreventionSystemEnabledForClient")]
        public Boolean BingePreventionSystemEnabledForClient { get; set; }

        [SerializedName("pendingBadges")]
        public Int32 PendingBadges { get; set; }

        [SerializedName("broadcastNotification")]
        public BroadcastNotification BroadcastNotification { get; set; }

        [SerializedName("minutesUntilMidnight")]
        public Int32 MinutesUntilMidnight { get; set; }

        [SerializedName("timeUntilFirstWinOfDay")]
        public Double TimeUntilFirstWinOfDay { get; set; }

        [SerializedName("coOpVsAiMsecsUntilReset")]
        public Double CoOpVsAiMsecsUntilReset { get; set; }

        [SerializedName("clientSystemStates")]
        public ClientSystemStatesNotification ClientSystemStates { get; set; }

        [SerializedName("bingeMinutesRemaining")]
        public Double BingeMinutesRemaining { get; set; }

        [SerializedName("pendingKudosDTO")]
        public PendingKudosDTO PendingKudosDTO { get; set; }

        [SerializedName("leaverBusterPenaltyTime")]
        public Int32 LeaverBusterPenaltyTime { get; set; }

        [SerializedName("platformId")]
        public String PlatformId { get; set; }

        [SerializedName("matchMakingEnabled")]
        public Boolean MatchMakingEnabled { get; set; }

        [SerializedName("minutesUntilShutdownEnabled")]
        public Boolean MinutesUntilShutdownEnabled { get; set; }

        [SerializedName("rpBalance")]
        public Double RpBalance { get; set; }

        [SerializedName("gameTypeConfigs")]
        public List<GameTypeConfigDTO> GameTypeConfigs { get; set; }

        [SerializedName("bingeIsPlayerInBingePreventionWindow")]
        public Boolean BingeIsPlayerInBingePreventionWindow { get; set; }

        [SerializedName("minorShutdownEnforced")]
        public Boolean MinorShutdownEnforced { get; set; }

        [SerializedName("competitiveRegion")]
        public String CompetitiveRegion { get; set; }

        [SerializedName("customMsecsUntilReset")]
        public Double CustomMsecsUntilReset { get; set; }
    }
}
