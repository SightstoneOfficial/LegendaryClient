using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Team.Dto
{
    public class RosterDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.team.dto.RosterDTO";

        public RosterDTO()
        {
        }

        public RosterDTO(Callback callback)
        {
            this.callback = callback;
        }

        public RosterDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(RosterDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("ownerId")]
        public Double OwnerId { get; set; }

        [InternalName("memberList")]
        public List<TeamMemberInfoDTO> MemberList { get; set; }
    }
}