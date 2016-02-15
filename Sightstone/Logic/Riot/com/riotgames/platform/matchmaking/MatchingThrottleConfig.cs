﻿using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.matchmaking.MatchingThrottleConfig")]
    public class MatchingThrottleConfig
    {
        [SerializedName("limit")]
        public double Limit { get; set; }

        [SerializedName("matchingThrottleProperties")]
        public List<object> MatchingThrottleProperties { get; set; }

        [SerializedName("cacheName")]
        public String CacheName { get; set; }
    }
}
