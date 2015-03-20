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
        public int MinNumPlayersForPracticeGame { get; set; }

        [InternalName("practiceGameTypeConfigIdList")]
        public int[] PracticeGameTypeConfigIdList { get; set; }

        [InternalName("freeToPlayChampionIdList")]
        public int[] FreeToPlayChampionIdList { get; set; }

        [InternalName("inactiveChampionIdList")]
        public object[] InactiveChampionIdList { get; set; }

        [InternalName("inactiveSpellIdList")]
        public int[] InactiveSpellIdList { get; set; }

        [InternalName("inactiveTutorialSpellIdList")]
        public int[] InactiveTutorialSpellIdList { get; set; }

        [InternalName("inactiveClassicSpellIdList")]
        public int[] InactiveClassicSpellIdList { get; set; }

        [InternalName("inactiveOdinSpellIdList")]
        public int[] InactiveOdinSpellIdList { get; set; }

        [InternalName("inactiveAramSpellIdList")]
        public int[] InactiveAramSpellIdList { get; set; }

        [InternalName("enabledQueueIdsList")]
        public int[] EnabledQueueIdsList { get; set; }

        [InternalName("unobtainableChampionSkinIDList")]
        public int[] UnobtainableChampionSkinIDList { get; set; }

        [InternalName("archivedStatsEnabled")]
        public Boolean ArchivedStatsEnabled { get; set; }

        [InternalName("queueThrottleDTO")]
        public Dictionary<string, object> QueueThrottleDTO { get; set; }

        [InternalName("gameMapEnabledDTOList")]
        public List<Dictionary<string, object>> GameMapEnabledDTOList { get; set; }

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
        public int SpectatorSlotLimit { get; set; }

        [InternalName("clientHeartBeatRateSeconds")]
        public int ClientHeartBeatRateSeconds { get; set; }

        [InternalName("observableGameModes")]
        public string[] ObservableGameModes { get; set; }

        [InternalName("observableCustomGameModes")]
        public string ObservableCustomGameModes { get; set; }

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
        public int MaxMasteryPagesOnServer { get; set; }

        [InternalName("tournamentSendStatsEnabled")]
        public Boolean TournamentSendStatsEnabled { get; set; }

        [InternalName("replayServiceAddress")]
        public string ReplayServiceAddress { get; set; }

        [InternalName("kudosEnabled")]
        public Boolean KudosEnabled { get; set; }

        [InternalName("buddyNotesEnabled")]
        public Boolean BuddyNotesEnabled { get; set; }

        [InternalName("localeSpecificChatRoomsEnabled")]
        public Boolean LocaleSpecificChatRoomsEnabled { get; set; }

        [InternalName("replaySystemStates")]
        public Dictionary<string, object> ReplaySystemStates { get; set; }

        [InternalName("sendFeedbackEventsEnabled")]
        public Boolean SendFeedbackEventsEnabled { get; set; }

        [InternalName("knownGeographicGameServerRegions")]
        public string[] KnownGeographicGameServerRegions { get; set; }

        [InternalName("leaguesDecayMessagingEnabled")]
        public Boolean LeaguesDecayMessagingEnabled { get; set; }
    }
}