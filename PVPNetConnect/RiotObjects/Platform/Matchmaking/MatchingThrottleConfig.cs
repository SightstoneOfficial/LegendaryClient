using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Matchmaking
{
    public class MatchingThrottleConfig : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.matchmaking.MatchingThrottleConfig";

        public MatchingThrottleConfig()
        {
        }

        public MatchingThrottleConfig(Callback callback)
        {
            this.callback = callback;
        }

        public MatchingThrottleConfig(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(MatchingThrottleConfig result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("limit")]
        public Double Limit { get; set; }

        [InternalName("matchingThrottleProperties")]
        public List<object> MatchingThrottleProperties { get; set; }

        [InternalName("cacheName")]
        public String CacheName { get; set; }
    }
}