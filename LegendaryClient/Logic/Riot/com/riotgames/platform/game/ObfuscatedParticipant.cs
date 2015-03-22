using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.ObfuscatedParticipant")]
    public class ObfuscatedParticipant : Participant
    {
        [SerializedName("badges")]
        public Int32 Badges { get; set; }

        [SerializedName("index")]
        public Int32 Index { get; set; }

        [SerializedName("clientInSynch")]
        public Boolean ClientInSynch { get; set; }

        [SerializedName("gameUniqueId")]
        public Int32 GameUniqueId { get; set; }

        [SerializedName("pickMode")]
        public Int32 PickMode { get; set; }
    }
}
