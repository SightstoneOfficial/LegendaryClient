using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.matchmaking.GameQueueConfig")]
    public class GameQueueConfig
    {
        [SerializedName("blockedMinutesThreshold")]
        public int BlockedMinutesThreshold { get; set; }

        [SerializedName("minimumParticipantListSize")]
        public int MinimumParticipantListSize { get; set; }

        [SerializedName("ranked")]
        public bool Ranked { get; set; }

        [SerializedName("maxLevel")]
        public int MaxLevel { get; set; }

        [SerializedName("minLevel")]
        public int MinLevel { get; set; }

        [SerializedName("gameTypeConfigId")]
        public int GameTypeConfigId { get; set; }

        [SerializedName("thresholdEnabled")]
        public bool ThresholdEnabled { get; set; }

        [SerializedName("queueState")]
        public String QueueState { get; set; }

        [SerializedName("type")]
        public String Type { get; set; }

        [SerializedName("cacheName")]
        public String CacheName { get; set; }

        [SerializedName("id")]
        public double Id { get; set; }

        [SerializedName("queueBonusKey")]
        public String QueueBonusKey { get; set; }

        [SerializedName("queueStateString")]
        public String QueueStateString { get; set; }

        [SerializedName("pointsConfigKey")]
        public String PointsConfigKey { get; set; }

        [SerializedName("teamOnly")]
        public bool TeamOnly { get; set; }

        [SerializedName("minimumQueueDodgeDelayTime")]
        public int MinimumQueueDodgeDelayTime { get; set; }

        [SerializedName("supportedMapIds")]
        public List<int> SupportedMapIds { get; set; }

        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("typeString")]
        public String TypeString { get; set; }

        [SerializedName("numPlayersPerTeam")]
        public int NumPlayersPerTeam { get; set; }

        [SerializedName("maximumParticipantListSize")]
        public int MaximumParticipantListSize { get; set; }

        [SerializedName("disallowFreeChampions")]
        public bool DisallowFreeChampions { get; set; }

        [SerializedName("mapSelectionAlgorithm")]
        public String MapSelectionAlgorithm { get; set; }

        [SerializedName("thresholdSize")]
        public double ThresholdSize { get; set; }

        [SerializedName("matchingThrottleConfig")]
        public MatchingThrottleConfig MatchingThrottleConfig { get; set; }
    }
}
