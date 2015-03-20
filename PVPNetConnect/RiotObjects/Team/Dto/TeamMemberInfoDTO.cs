using System;

namespace PVPNetConnect.RiotObjects.Team.Dto
{
    public class TeamMemberInfoDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.team.dto.TeamMemberInfoDTO";

        public TeamMemberInfoDTO()
        {
        }

        public TeamMemberInfoDTO(Callback callback)
        {
            this.callback = callback;
        }

        public TeamMemberInfoDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TeamMemberInfoDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("joinDate")]
        public DateTime JoinDate { get; set; }

        [InternalName("playerName")]
        public string PlayerName { get; set; }

        [InternalName("inviteDate")]
        public DateTime InviteDate { get; set; }

        [InternalName("status")]
        public string Status { get; set; }

        [InternalName("playerId")]
        public Double PlayerId { get; set; }
    }
}