using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.RuneSlot")]
    public class RuneSlot
    {
        [SerializedName("id")]
        public int Id { get; set; }

        [SerializedName("minLevel")]
        public int MinLevel { get; set; }

        [SerializedName("runeType")]
        public RuneType RuneType { get; set; }
    }
}
