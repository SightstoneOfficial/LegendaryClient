using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.matchmaking.BustedLeaver")]
    public class BustedLeaver : QueueDodger
    {
        //[SerializedName("reasonFailed")]		
        //public string ReasonFailed { get; set; }

        [SerializedName("accessToken")]		
        public string AccessToken { get; set; }

        [SerializedName("leaverPenaltyMillisRemaining")]		
        public double LeaverPenaltyMilisRemaining { get; set; }

        //[SerializedName("summoner")]		
        //public Summoner Summoner { get; set; }
    }
}
