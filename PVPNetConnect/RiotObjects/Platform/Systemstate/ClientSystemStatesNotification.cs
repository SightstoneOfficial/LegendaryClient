using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Systemstate
{
    public class ClientSystemStatesNotification : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.systemstate.ClientSystemStatesNotification";

        public ClientSystemStatesNotification()
        {
        }

        public ClientSystemStatesNotification(Callback callback)
        {
            this.callback = callback;
        }

        public ClientSystemStatesNotification(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(ClientSystemStatesNotification result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("championTradeThroughLCDS")]
        public Boolean ChampionTradeThroughLCDS { get; set; }

        [InternalName("practiceGameEnabled")]
        public Boolean PracticeGameEnabled { get; set; }

        [InternalName("advancedTutorialEnabled")]
        public Boolean AdvancedTutorialEnabled { get; set; }

        [InternalName("minNumPlayersForPracticeGame")]
        public Int32 MinNumPlayersForPracticeGame { get; set; }

        [InternalName("practiceGameTypeConfigIdList")]
        public Int32[] PracticeGameTypeConfigIdList { get; set; }

        [InternalName("freeToPlayChampionIdList")]
        public Int32[] FreeToPlayChampionIdList { get; set; }

        [InternalName("inactiveChampionIdList")]
        public object[] InactiveChampionIdList { get; set; }

        [InternalName("inactiveSpellIdList")]
        public Int32[] InactiveSpellIdList { get; set; }

        [InternalName("inactiveTutorialSpellIdList")]
        public Int32[] InactiveTutorialSpellIdList { get; set; }

        [InternalName("inactiveClassicSpellIdList")]
        public Int32[] InactiveClassicSpellIdList { get; set; }

        [InternalName("inactiveOdinSpellIdList")]
        public Int32[] InactiveOdinSpellIdList { get; set; }

        [InternalName("inactiveAramSpellIdList")]
        public Int32[] InactiveAramSpellIdList { get; set; }

        [InternalName("enabledQueueIdsList")]
        public Int32[] EnabledQueueIdsList { get; set; }

        [InternalName("unobtainableChampionSkinIDList")]
        public Int32[] UnobtainableChampionSkinIDList { get; set; }

        [InternalName("archivedStatsEnabled")]
        public Boolean ArchivedStatsEnabled { get; set; }

        [InternalName("queueThrottleDTO")]
        public Dictionary<String, Object> QueueThrottleDTO { get; set; }

        [InternalName("gameMapEnabledDTOList")]
        public List<Dictionary<String, Object>> GameMapEnabledDTOList { get; set; }

        [InternalName("storeCustomerEnabled")]
        public Boolean StoreCustomerEnabled { get; set; }

        [InternalName("socialIntegrationEnabled")]
        public Boolean SocialIntegrationEnabled { get; set; }

        [InternalName("runeUniquePerSpellBook")]
        public Boolean RuneUniquePerSpellBook { get; set; }

        [InternalName("tribunalEnabled")]
        public Boolean TribunalEnabled { get; set; }

        [InternalName("observerModeEnabled")]
        public Boolean ObserverModeEnabled { get; set; }

        [InternalName("spectatorSlotLimit")]
        public Int32 SpectatorSlotLimit { get; set; }

        [InternalName("clientHeartBeatRateSeconds")]
        public Int32 ClientHeartBeatRateSeconds { get; set; }

        [InternalName("observableGameModes")]
        public String[] ObservableGameModes { get; set; }

        [InternalName("observableCustomGameModes")]
        public String ObservableCustomGameModes { get; set; }

        [InternalName("teamServiceEnabled")]
        public Boolean TeamServiceEnabled { get; set; }

        [InternalName("leagueServiceEnabled")]
        public Boolean LeagueServiceEnabled { get; set; }

        [InternalName("modularGameModeEnabled")]
        public Boolean ModularGameModeEnabled { get; set; }

        [InternalName("riotDataServiceDataSendProbability")]
        public int RiotDataServiceDataSendProbability { get; set; }

        [InternalName("displayPromoGamesPlayedEnabled")]
        public Boolean DisplayPromoGamesPlayedEnabled { get; set; }

        [InternalName("masteryPageOnServer")]
        public Boolean MasteryPageOnServer { get; set; }

        [InternalName("maxMasteryPagesOnServer")]
        public Int32 MaxMasteryPagesOnServer { get; set; }

        [InternalName("tournamentSendStatsEnabled")]
        public Boolean TournamentSendStatsEnabled { get; set; }

        [InternalName("replayServiceAddress")]
        public String ReplayServiceAddress { get; set; }

        [InternalName("kudosEnabled")]
        public Boolean KudosEnabled { get; set; }

        [InternalName("buddyNotesEnabled")]
        public Boolean BuddyNotesEnabled { get; set; }

        [InternalName("localeSpecificChatRoomsEnabled")]
        public Boolean LocaleSpecificChatRoomsEnabled { get; set; }

        [InternalName("replaySystemStates")]
        public Dictionary<String, Object> ReplaySystemStates { get; set; }

        [InternalName("sendFeedbackEventsEnabled")]
        public Boolean SendFeedbackEventsEnabled { get; set; }

        [InternalName("knownGeographicGameServerRegions")]
        public String[] KnownGeographicGameServerRegions { get; set; }

        [InternalName("leaguesDecayMessagingEnabled")]
        public Boolean LeaguesDecayMessagingEnabled { get; set; }
    }
}