using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.SummonerTalentsAndPoints")]
    public class SummonerTalentsAndPoints
    {
        [SerializedName("talentPoints")]
        public int TalentPoints { get; set; }

        [SerializedName("modifyDate")]
        public DateTime ModifyDate { get; set; }

        [SerializedName("createDate")]
        public DateTime CreateDate { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }
    }
}
