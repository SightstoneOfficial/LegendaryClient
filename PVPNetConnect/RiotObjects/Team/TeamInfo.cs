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
        public double SecondsUntilEligibleForDeletion { get; set; }

        [InternalName("memberStatusString")]
        public string MemberStatusString { get; set; }

        [InternalName("tag")]
        public string Tag { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("memberStatus")]
        public string MemberStatus { get; set; }

        [InternalName("teamId")]
        public TeamId TeamId { get; set; }
    }
}