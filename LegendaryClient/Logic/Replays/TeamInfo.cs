#region

using System;
using RtmpSharp.IO;

#endregion

namespace LegendaryClient.Logic.Replays
{
    [Serializable]
    [SerializedName("com.riotgames.team.TeamInfo")]
    public class TeamInfo
    {
        [SerializedName("secondsUntilEligibleForDeletion")]
        public Double SecondsUntilEligibleForDeletion { get; set; }

        [SerializedName("memberStatusString")]
        public String MemberStatusString { get; set; }

        [SerializedName("tag")]
        public String Tag { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("memberStatus")]
        public String MemberStatus { get; set; }

        [SerializedName("teamId")]
        public TeamId TeamId { get; set; }
    }
}