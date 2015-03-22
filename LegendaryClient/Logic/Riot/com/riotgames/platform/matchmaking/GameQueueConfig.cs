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
        public Int32 BlockedMinutesThreshold { get; set; }

        [SerializedName("minimumParticipantListSize")]
        public Int32 MinimumParticipantListSize { get; set; }

        [SerializedName("ranked")]
        public Boolean Ranked { get; set; }

        [SerializedName("maxLevel")]
        public Int32 MaxLevel { get; set; }

        [SerializedName("minLevel")]
        public Int32 MinLevel { get; set; }

        [SerializedName("gameTypeConfigId")]
        public Int32 GameTypeConfigId { get; set; }

        [SerializedName("thresholdEnabled")]
        public Boolean ThresholdEnabled { get; set; }

        [SerializedName("queueState")]
        public String QueueState { get; set; }

        [SerializedName("type")]
        public String Type { get; set; }

        [SerializedName("cacheName")]
        public String CacheName { get; set; }

        [SerializedName("id")]
        public Double Id { get; set; }

        [SerializedName("queueBonusKey")]
        public String QueueBonusKey { get; set; }

        [SerializedName("queueStateString")]
        public String QueueStateString { get; set; }

        [SerializedName("pointsConfigKey")]
        public String PointsConfigKey { get; set; }

        [SerializedName("teamOnly")]
        public Boolean TeamOnly { get; set; }

        [SerializedName("minimumQueueDodgeDelayTime")]
        public Int32 MinimumQueueDodgeDelayTime { get; set; }

        [SerializedName("supportedMapIds")]
        public List<Int32> SupportedMapIds { get; set; }

        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("typeString")]
        public String TypeString { get; set; }

        [SerializedName("numPlayersPerTeam")]
        public Int32 NumPlayersPerTeam { get; set; }

        [SerializedName("maximumParticipantListSize")]
        public Int32 MaximumParticipantListSize { get; set; }

        [SerializedName("disallowFreeChampions")]
        public Boolean DisallowFreeChampions { get; set; }

        [SerializedName("mapSelectionAlgorithm")]
        public String MapSelectionAlgorithm { get; set; }

        [SerializedName("thresholdSize")]
        public Double ThresholdSize { get; set; }

        [SerializedName("matchingThrottleConfig")]
        public MatchingThrottleConfig MatchingThrottleConfig { get; set; }
    }
}
