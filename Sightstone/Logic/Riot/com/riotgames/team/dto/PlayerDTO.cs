using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Team
{
    [Serializable]
    [SerializedName("com.riotgames.team.dto.PlayerDTO")]
    public class PlayerDTO
    {
        [SerializedName("playerId")]
        public double PlayerId { get; set; }

        [SerializedName("teamsSummary")]
        public List<object> TeamsSummary { get; set; }

        [SerializedName("createdTeams")]
        public List<object> CreatedTeams { get; set; }

        [SerializedName("playerTeams")]
        public List<object> PlayerTeams { get; set; }
    }
}
