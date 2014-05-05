using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Trade
{
    public class PotentialTradersDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.trade.api.contract.PotentialTradersDTO";

        public PotentialTradersDTO()
        {
        }

        public PotentialTradersDTO(Callback callback)
        {
            this.callback = callback;
        }

        public PotentialTradersDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PotentialTradersDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("potentialTraders")]
        public List<String> PotentialTraders { get; set; }
    }
}
