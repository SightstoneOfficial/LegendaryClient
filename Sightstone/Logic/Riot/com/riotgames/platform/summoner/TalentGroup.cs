using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.TalentGroup")]
    public class TalentGroup
    {
        [SerializedName("index")]
        public int Index { get; set; }

        [SerializedName("talentRows")]
        public List<TalentRow> TalentRows { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("tltGroupId")]
        public int TltGroupId { get; set; }
    }
}
