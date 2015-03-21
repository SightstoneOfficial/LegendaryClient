using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPNetConnect.RiotObjects.Platform.Trade
{
    public class TradeContractDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.trade.api.contract.TradeContractDTO";

        public TradeContractDTO()
        {
        }

        public TradeContractDTO(Callback callback)
        {
            this.callback = callback;
        }

        public TradeContractDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TradeContractDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("requesterInternalSummonerName")]
        public string RequesterInternalSummonerName { get; set; }

        [InternalName("requesterChampionId")]
        public double RequesterChampionId { get; set; }

        [InternalName("state")]
        public string State { get; set; }

        [InternalName("responderChampionId")]
        public double ResponderChampionId { get; set; }

        [InternalName("responderInternalSummonerName")]
        public string ResponderInternalSummonerName { get; set; }
    }
}
