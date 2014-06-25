using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    class Owner : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.gameinvite.contract.LobbyStatus";

        public Owner()
        {
        }

        public Owner(Callback callback)
        {
            this.callback = callback;
        }

        public Owner(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Owner result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}
