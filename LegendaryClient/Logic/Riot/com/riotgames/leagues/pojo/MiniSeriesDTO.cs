using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Leagues
{
    [Serializable]
    [SerializedName("com.riotgames.leagues.pojo.MiniSeriesDTO")]
    public class MiniSeriesDTO
    {
        [SerializedName("progress")]
        public String Progress { get; set; }

        [SerializedName("target")]
        public Int32 Target { get; set; }

        [SerializedName("losses")]
        public Int32 Losses { get; set; }

        [SerializedName("timeLeftToPlayMillis")]
        public Double TimeLeftToPlayMillis { get; set; }

        [SerializedName("wins")]
        public Int32 Wins { get; set; }
    }
}
