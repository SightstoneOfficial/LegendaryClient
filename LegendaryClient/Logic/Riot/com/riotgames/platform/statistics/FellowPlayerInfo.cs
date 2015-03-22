using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.FellowPlayerInfo")]
    public class FellowPlayerInfo
    {
        [SerializedName("championId")]
        public Double ChampionId { get; set; }

        [SerializedName("teamId")]
        public Int32 TeamId { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }
    }
}
