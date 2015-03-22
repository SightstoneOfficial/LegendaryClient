using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.TalentGroup")]
    public class TalentGroup
    {
        [SerializedName("index")]
        public Int32 Index { get; set; }

        [SerializedName("talentRows")]
        public List<TalentRow> TalentRows { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("tltGroupId")]
        public Int32 TltGroupId { get; set; }
    }
}
