using PVPNetConnect.RiotObjects.Team.Stats;
using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Team.Dto
{
    public class TeamDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.team.dto.TeamDTO";

        public TeamDTO()
        {
        }

        public TeamDTO(Callback callback)
        {
            this.callback = callback;
        }

        public TeamDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TeamDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("teamStatSummary")]
        public TeamStatSummary TeamStatSummary { get; set; }

        [InternalName("status")]
        public String Status { get; set; }

        [InternalName("tag")]
        public String Tag { get; set; }

        [InternalName("roster")]
        public RosterDTO Roster { get; set; }

        [InternalName("lastGameDate")]
        public object LastGameDate { get; set; }

        [InternalName("modifyDate")]
        public DateTime ModifyDate { get; set; }

        [InternalName("messageOfDay")]
        public object MessageOfDay { get; set; }

        [InternalName("teamId")]
        public TeamId TeamId { get; set; }

        [InternalName("lastJoinDate")]
        public DateTime LastJoinDate { get; set; }

        [InternalName("secondLastJoinDate")]
        public DateTime SecondLastJoinDate { get; set; }

        [InternalName("secondsUntilEligibleForDeletion")]
        public Double SecondsUntilEligibleForDeletion { get; set; }

        [InternalName("matchHistory")]
        public List<object> MatchHistory { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("thirdLastJoinDate")]
        public DateTime ThirdLastJoinDate { get; set; }

        [InternalName("createDate")]
        public DateTime CreateDate { get; set; }
    }
}