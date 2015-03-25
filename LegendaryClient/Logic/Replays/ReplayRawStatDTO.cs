using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Replays
{
    [Serializable]
    [SerializedName("com.riotgames.platform.observer.domain.RawStatDTO")]
    public class ReplayRawStatDTO
    {
        [SerializedName("value")]
        public double Value { get; set; }

        [SerializedName("statTypeName")]
        public string StatTypeName { get; set; }
    }
}
