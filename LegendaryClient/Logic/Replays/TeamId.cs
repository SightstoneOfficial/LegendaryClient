using RtmpSharp.IO;
using System;

namespace LegendaryClient.Logic.Replays
{
    [Serializable]
    [SerializedName("com.riotgames.team.TeamId")]
    public class TeamId
    {
        [SerializedName("fullId")]
        public string FullId { get; set; }
    }
}