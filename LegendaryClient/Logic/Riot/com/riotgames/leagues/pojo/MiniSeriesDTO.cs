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
        public int Target { get; set; }

        [SerializedName("losses")]
        public int Losses { get; set; }

        [SerializedName("timeLeftToPlayMillis")]
        public double TimeLeftToPlayMillis { get; set; }

        [SerializedName("wins")]
        public int Wins { get; set; }
    }
}
