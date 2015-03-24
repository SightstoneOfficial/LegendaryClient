using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Kudos
{
    [Serializable]
    [SerializedName("com.riotgames.kudos.dto.PendingKudosDTO")]
    public class PendingKudosDTO
    {
        [SerializedName("pendingCounts")]
        public int[] PendingCounts { get; set; }
    }
}
