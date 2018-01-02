using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.icon.SummonerIconInventoryDTO")]
    public class SummonerIconInventoryDTO
    {
        [SerializedName("summonerId")]
        public double SummonerId { get; set; }

        [SerializedName("summonerIcons")]
        public List<Icon> SummonerIcons { get; set; }
    }
}
