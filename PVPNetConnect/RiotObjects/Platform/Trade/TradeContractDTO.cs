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
        public String RequesterInternalSummonerName { get; set; }

        [InternalName("requesterChampionId")]
        public Double RequesterChampionId { get; set; }

        [InternalName("state")]
        public String State { get; set; }

        [InternalName("responderChampionId")]
        public Double ResponderChampionId { get; set; }

        [InternalName("responderInternalSummonerName")]
        public String ResponderInternalSummonerName { get; set; }
    }
}
