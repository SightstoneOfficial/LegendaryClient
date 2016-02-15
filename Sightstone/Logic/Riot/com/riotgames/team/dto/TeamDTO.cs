using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Team
{
    [Serializable]
    [SerializedName("com.riotgames.team.dto.TeamDTO")]
    public class TeamDTO
    {
        [SerializedName("teamStatSummary")]
        public TeamStatSummary TeamStatSummary { get; set; }

        [SerializedName("status")]
        public String Status { get; set; }

        [SerializedName("tag")]
        public String Tag { get; set; }

        [SerializedName("roster")]
        public RosterDTO Roster { get; set; }

        [SerializedName("lastGameDate")]
        public object LastGameDate { get; set; }

        [SerializedName("modifyDate")]
        public DateTime ModifyDate { get; set; }

        [SerializedName("messageOfDay")]
        public object MessageOfDay { get; set; }

        [SerializedName("teamId")]
        public TeamId TeamId { get; set; }

        [SerializedName("lastJoinDate")]
        public DateTime LastJoinDate { get; set; }

        [SerializedName("secondLastJoinDate")]
        public DateTime SecondLastJoinDate { get; set; }

        [SerializedName("secondsUntilEligibleForDeletion")]
        public double SecondsUntilEligibleForDeletion { get; set; }

        [SerializedName("matchHistory")]
        public List<object> MatchHistory { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("thirdLastJoinDate")]
        public DateTime ThirdLastJoinDate { get; set; }

        [SerializedName("createDate")]
        public DateTime CreateDate { get; set; }
    }
}
