using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Leagues
{
    [Serializable]
    [SerializedName("com.riotgames.leagues.pojo.LeagueListDTO")]
    public class LeagueListDTO
    {
        [SerializedName("queue")]
        public String Queue { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("tier")]
        public String Tier { get; set; }

        [SerializedName("requestorsRank")]
        public String RequestorsRank { get; set; }

        [SerializedName("entries")]
        public List<LeagueItemDTO> Entries { get; set; }

        [SerializedName("requestorsName")]
        public String RequestorsName { get; set; }
    }
}
