using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Kudos
{
    [Serializable]
    [SerializedName("com.riotgames.kudos.dto.PendingKudosDTO")]
    public class PendingKudosDTO
    {
        [SerializedName("pendingCounts")]
        public int[] PendingCounts { get; set; }
    }
}
