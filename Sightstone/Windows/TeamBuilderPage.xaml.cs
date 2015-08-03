using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using Sightstone.Logic.Riot.Platform;
using Sightstone.Logic.MultiUser;

namespace Sightstone.Windows
{
    /// <summary>
    ///     Interaction logic for TeamBuilder.xaml
    /// </summary>
    public partial class TeamBuilderPage
    {
        private static readonly UserClient _userClient = (UserList.Users[Client.CurrentServer])[Client.CurrentUser];

        /// <summary>
        ///     TOP
        ///     MIDDLE
        ///     BOTTOM
        ///     JUNGLE
        /// </summary>
        internal string Position;

        /// <summary>
        ///     Possible roles:
        ///     MAGE
        ///     SUPPORT
        ///     ASSASSIN
        ///     MARKSMAN
        ///     FIGHTER
        ///     TANK
        /// </summary>
        internal string Role;

        public TeamBuilderPage(bool iscreater, LobbyStatus myLobby)
        {
            InitializeComponent();
            StartUp();
        }

        static void StartUp()
        {
            Call(TeambuilderData.retrieveFeatureToggles, JsonConvert.SerializeObject(new retrieveFeatureToggles()));
            var queueId = new retrieveInfo { queueId = 61 };
            Call(TeambuilderData.retrieveInfoV1, JsonConvert.SerializeObject(queueId));
        }

        public static void Call(TeambuilderData data, string args)
        {
            _userClient.calls.CallLCDS(Guid.NewGuid().ToString(), "cap", data.ToString(), args);
        }
    }

    /// <summary>
    /// Lol its empty
    /// </summary>
    public class retrieveFeatureToggles
    {
        
    }

    /// <summary>
    /// Sugoi rito gems
    /// </summary>
    public class retrieveInfo
    {
        public int queueId { get; set; }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum TeambuilderData
    {
        retrieveFeatureToggles,
        retrieveInfoV1,
        updateLastSelectedSkinForChampionV1,
        retrieveEstimatedWaitTimeV2,
        createSoloQueryV5,
        indicateGroupAcceptanceAsCandidateV1,
        indicateReadinessV1,
    }
}