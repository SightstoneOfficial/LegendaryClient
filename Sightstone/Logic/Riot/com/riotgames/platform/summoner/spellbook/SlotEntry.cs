using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.spellbook.SlotEntry")]
    public class SlotEntry
    {
        [SerializedName("runeId")]
        public int RuneId { get; set; }

        [SerializedName("runeSlotId")]
        public int RuneSlotId { get; set; }
    }
}
