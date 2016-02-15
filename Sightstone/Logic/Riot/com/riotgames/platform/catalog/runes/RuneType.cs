using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.catalog.runes.RuneType")]
    public class RuneType
    {
        [SerializedName("runeTypeId")]
        public int RuneTypeId { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }
    }
}
