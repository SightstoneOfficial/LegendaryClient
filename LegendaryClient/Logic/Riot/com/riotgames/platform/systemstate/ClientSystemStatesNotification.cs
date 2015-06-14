using System;
using RtmpSharp.IO;
using System.Collections.Generic;
using RtmpSharp.IO.AMF3;
using System.Text;
using System.Linq;
using System.Web.Script.Serialization;
using System.Reflection;
using System.Collections;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.systemstate.ClientSystemStatesNotification")]
    public class ClientSystemStatesNotification : IExternalizable
    {
        public bool championTradeThroughLCDS { get; set; }

        public bool practiceGameEnabled { get; set; }

        public bool advancedTutorialEnabled { get; set; }

        public int[] practiceGameTypeConfigIdList { get; set; }

        public int minNumPlayersForPracticeGame { get; set; }

        public int[] PracticeGameTypeConfigIdList { get; set; }

        public int[] freeToPlayChampionIdList { get; set; }

        public int[] inactiveChampionIdList { get; set; }

        public int[] inactiveSpellIdList { get; set; }

        public int[] inactiveTutorialSpellIdList { get; set; }

        public int[] inactiveClassicSpellIdList { get; set; }

        public int[] inactiveOdinSpellIdList { get; set; }

        public int[] inactiveAramSpellIdList { get; set; }

        public int[] enabledQueueIdsList { get; set; }

        public int[] unobtainableChampionSkinIDList { get; set; }

        public int[] freeToPlayChampionForNewPlayersIdList { get; set; }

        public Dictionary<String, object> gameModeToInactiveSpellIds { get; set; }

        public bool archivedStatsEnabled { get; set; }

        public Dictionary<String, object> queueThrottleDTO { get; set; }

        public Dictionary<String, object>[] gameMapEnabledDTOList { get; set; }

        public bool storeCustomerEnabled { get; set; }

        public bool socialIntegrationEnabled { get; set; }

        public bool runeUniquePerSpellBook { get; set; }

        public bool tribunalEnabled { get; set; }

        public bool observerModeEnabled { get; set; }

        public int currentSeason { get; set; }

        public int freeToPlayChampionsForNewPlayersMaxLevel { get; set; }

        public int spectatorSlotLimit { get; set; }

        public int clientHeartBeatRateSeconds { get; set; }

        public String[] observableGameModes { get; set; }

        public String observableCustomGameModes { get; set; }

        public bool teamServiceEnabled { get; set; }

        public bool leagueServiceEnabled { get; set; }

        public bool modularGameModeEnabled { get; set; }

        public decimal riotDataServiceDataSendProbability { get; set; }

        public bool displayPromoGamesPlayedEnabled { get; set; }

        public bool masteryPageOnServer { get; set; }

        public int maxMasteryPagesOnServer { get; set; }

        public bool tournamentSendStatsEnabled { get; set; }

        public String replayServiceAddress { get; set; }

        public bool kudosEnabled { get; set; }

        public bool buddyNotesEnabled { get; set; }

        public bool localeSpecificChatRoomsEnabled { get; set; }

        public Dictionary<String, object> replaySystemStates { get; set; }

        public bool sendFeedbackEventsEnabled { get; set; }

        public String[] knownGeographicGameServerRegions { get; set; }

        public bool leaguesDecayMessagingEnabled { get; set; }

        public bool tournamentShortCodesEnabled { get; set; }

        public string Json { get; set; }

        public void ReadExternal(IDataInput input)
        {
            Json = input.ReadUtf((int)input.ReadUInt32());

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            Dictionary<string, object> deserializedJSON = serializer.Deserialize<Dictionary<string, object>>(Json);

            Type classType = typeof(ClientSystemStatesNotification);
            foreach (KeyValuePair<string, object> keyPair in deserializedJSON)
            {
                var f = classType.GetProperty(keyPair.Key);
                if (f == null)
                {
                    Client.Log("New Client System State: " + keyPair.Key);
                    continue;
                }
                if (keyPair.Value.GetType() == typeof(ArrayList))
                {
                    ArrayList tempArrayList = keyPair.Value as ArrayList;
                    if (tempArrayList.Count > 0)
                        f.SetValue(this, tempArrayList.ToArray(f.PropertyType.GetElementType()));
                    else
                        f.SetValue(this, null);
                }
                else
                {
                    f.SetValue(this, keyPair.Value);
                }
            }
        }

        public void WriteExternal(IDataOutput output)
        {
            var bytes = Encoding.UTF8.GetBytes(Json);

            output.WriteInt32(bytes.Length);
            output.WriteBytes(bytes);
        }
    }
}
