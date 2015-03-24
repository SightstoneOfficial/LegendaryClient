using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.matchmaking.QueueInfo")]
    public class QueueInfo
    {
        [SerializedName("waitTime")]
        public double WaitTime { get; set; }

        [SerializedName("queueId")]
        public double QueueId { get; set; }

        [SerializedName("queueLength")]
        public int QueueLength { get; set; }
    }
}
