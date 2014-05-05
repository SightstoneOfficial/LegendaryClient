using System;

namespace PVPNetConnect.RiotObjects.Team
{
    public class TeamId : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.team.TeamId";

        public TeamId()
        {
        }

        public TeamId(Callback callback)
        {
            this.callback = callback;
        }

        public TeamId(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TeamId result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("fullId")]
        public String FullId { get; set; }
    }
}