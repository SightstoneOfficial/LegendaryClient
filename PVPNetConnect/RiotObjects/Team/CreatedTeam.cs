using System;

namespace PVPNetConnect.RiotObjects.Team
{
    public class CreatedTeam : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.team.CreatedTeam";

        public CreatedTeam()
        {
        }

        public CreatedTeam(Callback callback)
        {
            this.callback = callback;
        }

        public CreatedTeam(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(CreatedTeam result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("timeStamp")]
        public Double TimeStamp { get; set; }

        [InternalName("teamId")]
        public TeamId TeamId { get; set; }
    }
}