using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.matchmaking.SearchingForMatchNotification")]
    public class SearchingForMatchNotification
    {
        [SerializedName("playerJoinFailures")]
        public List<QueueDodger> PlayerJoinFailures { get; set; }

        [SerializedName("ghostGameSummoners")]
        public object GhostGameSummoners { get; set; }

        [SerializedName("joinedQueues")]
        public List<QueueInfo> JoinedQueues { get; set; }
    }
}
