using System;

namespace PVPNetConnect.RiotObjects.Team
{
    public class TeamInfo : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.team.TeamInfo";

        public TeamInfo()
        {
        }

        public TeamInfo(Callback callback)
        {
            this.callback = callback;
        }

        public TeamInfo(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TeamInfo result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("secondsUntilEligibleForDeletion")]
        public Double SecondsUntilEligibleForDeletion { get; set; }

        [InternalName("memberStatusString")]
        public String MemberStatusString { get; set; }

        [InternalName("tag")]
        public String Tag { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("memberStatus")]
        public String MemberStatus { get; set; }

        [InternalName("teamId")]
        public TeamId TeamId { get; set; }
    }
}