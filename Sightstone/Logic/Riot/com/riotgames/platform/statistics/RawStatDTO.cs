using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.statistics.RawStatDTO")]
    public class RawStatDTO
    {
        [SerializedName("value")]
        public double Value { get; set; }

        [SerializedName("statTypeName")]
        public String StatTypeName { get; set; }
    }
}
