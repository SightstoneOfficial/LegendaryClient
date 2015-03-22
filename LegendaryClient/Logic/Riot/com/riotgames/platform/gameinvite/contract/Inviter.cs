using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.gameinvite.contract.Inviter")]
    public class Inviter
    {
        [SerializedName("previousSeasonHighestTier")]
        public string PreviousSeasonHighestTier { get; set; }

        [SerializedName("summonerName")]
        public string SummonerName { get; set; }

        [SerializedName("summonerId")]
        public int SummonerId { get; set; }
    }
}
