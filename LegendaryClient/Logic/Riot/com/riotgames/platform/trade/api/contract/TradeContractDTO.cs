using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.trade.api.contract.TradeContractDTO")]
    public class TradeContractDTO
    {
        [SerializedName("requesterInternalSummonerName")]
        public String RequesterInternalSummonerName { get; set; }

        [SerializedName("requesterChampionId")]
        public double RequesterChampionId { get; set; }

        [SerializedName("state")]
        public String State { get; set; }

        [SerializedName("responderChampionId")]
        public double ResponderChampionId { get; set; }

        [SerializedName("responderInternalSummonerName")]
        public String ResponderInternalSummonerName { get; set; }
    }
}
