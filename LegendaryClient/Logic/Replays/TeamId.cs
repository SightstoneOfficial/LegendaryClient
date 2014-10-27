using RtmpSharp.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.Replays
{
    [Serializable]
    [SerializedName("com.riotgames.team.TeamId")]
    public class TeamId
    {
        [SerializedName("fullId")]
        public String FullId { get; set; }
    }
}
