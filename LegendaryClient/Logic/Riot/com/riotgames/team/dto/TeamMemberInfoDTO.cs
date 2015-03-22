using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Team
{
    [Serializable]
    [SerializedName("com.riotgames.team.dto.TeamMemberInfoDTO")]
    public class TeamMemberInfoDTO
    {
        [SerializedName("joinDate")]
        public DateTime JoinDate { get; set; }

        [SerializedName("playerName")]
        public String PlayerName { get; set; }

        [SerializedName("inviteDate")]
        public DateTime InviteDate { get; set; }

        [SerializedName("status")]
        public String Status { get; set; }

        [SerializedName("playerId")]
        public Double PlayerId { get; set; }
    }
}
