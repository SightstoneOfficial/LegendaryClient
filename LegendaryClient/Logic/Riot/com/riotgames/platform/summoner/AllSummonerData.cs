using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.AllSummonerData")]
    public class AllSummonerData
    {
        [SerializedName("spellBook")]
        public SpellBookDTO SpellBook { get; set; }

        [SerializedName("summonerDefaultSpells")]
        public SummonerDefaultSpells SummonerDefaultSpells { get; set; }

        [SerializedName("summonerTalentsAndPoints")]
        public SummonerTalentsAndPoints SummonerTalentsAndPoints { get; set; }

        [SerializedName("summoner")]
        public Summoner Summoner { get; set; }

        [SerializedName("masteryBook")]
        public MasteryBookDTO MasteryBook { get; set; }

        [SerializedName("summonerLevelAndPoints")]
        public SummonerLevelAndPoints SummonerLevelAndPoints { get; set; }

        [SerializedName("summonerLevel")]
        public SummonerLevel SummonerLevel { get; set; }
    }
}
