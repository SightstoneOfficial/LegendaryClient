using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Team
{
    [Serializable]
    [SerializedName("com.riotgames.team.CreatedTeam")]
    public class CreatedTeam
    {
        [SerializedName("timeStamp")]
        public double TimeStamp { get; set; }

        [SerializedName("teamId")]
        public TeamId TeamId { get; set; }
    }
}
