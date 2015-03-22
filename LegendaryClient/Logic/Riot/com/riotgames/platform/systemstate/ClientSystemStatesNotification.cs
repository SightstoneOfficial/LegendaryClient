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
        public Boolean championTradeThroughLCDS { get; set; }

        public Boolean practiceGameEnabled { get; set; }

        public Boolean advancedTutorialEnabled { get; set; }

        public Int32[] practiceGameTypeConfigIdList { get; set; }

        public Int32 minNumPlayersForPracticeGame { get; set; }

        public Int32[] PracticeGameTypeConfigIdList { get; set; }

        public Int32[] freeToPlayChampionIdList { get; set; }

        public object[] inactiveChampionIdList { get; set; }

        public Int32[] inactiveSpellIdList { get; set; }

        public Int32[] inactiveTutorialSpellIdList { get; set; }

        public Int32[] inactiveClassicSpellIdList { get; set; }

        public Int32[] inactiveOdinSpellIdList { get; set; }

        public Int32[] inactiveAramSpellIdList { get; set; }

        public Int32[] enabledQueueIdsList { get; set; }

        public Int32[] unobtainableChampionSkinIDList { get; set; }

        public Int32[] freeToPlayChampionForNewPlayersIdList { get; set; }

        public Dictionary<String, Object> gameModeToInactiveSpellIds { get; set; }

        public Boolean archivedStatsEnabled { get; set; }

        public Dictionary<String, Object> queueThrottleDTO { get; set; }

        public Dictionary<String, Object>[] gameMapEnabledDTOList { get; set; }

        public Boolean storeCustomerEnabled { get; set; }

        public Boolean socialIntegrationEnabled { get; set; }

        public Boolean runeUniquePerSpellBook { get; set; }

        public Boolean tribunalEnabled { get; set; }

        public Boolean observerModeEnabled { get; set; }

        public Int32 currentSeason { get; set; }

        public Int32 freeToPlayChampionsForNewPlayersMaxLevel { get; set; }

        public Int32 spectatorSlotLimit { get; set; }

        public Int32 clientHeartBeatRateSeconds { get; set; }

        public String[] observableGameModes { get; set; }

        public String observableCustomGameModes { get; set; }

        public Boolean teamServiceEnabled { get; set; }

        public Boolean leagueServiceEnabled { get; set; }

        public Boolean modularGameModeEnabled { get; set; }

        public Decimal riotDataServiceDataSendProbability { get; set; }

        public Boolean displayPromoGamesPlayedEnabled { get; set; }

        public Boolean masteryPageOnServer { get; set; }

        public Int32 maxMasteryPagesOnServer { get; set; }

        public Boolean tournamentSendStatsEnabled { get; set; }

        public String replayServiceAddress { get; set; }

        public Boolean kudosEnabled { get; set; }

        public Boolean buddyNotesEnabled { get; set; }

        public Boolean localeSpecificChatRoomsEnabled { get; set; }

        public Dictionary<String, Object> replaySystemStates { get; set; }

        public Boolean sendFeedbackEventsEnabled { get; set; }

        public String[] knownGeographicGameServerRegions { get; set; }

        public Boolean leaguesDecayMessagingEnabled { get; set; }

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
                if (keyPair.Value.GetType() == typeof(ArrayList))
                {
                    ArrayList tempArrayList = keyPair.Value as ArrayList;
                    if (tempArrayList.Count > 0)
                        f.SetValue(this, ((ArrayList)keyPair.Value).ToArray(tempArrayList[0].GetType()));
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
