using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    public class Player : RiotGamesObject
    {
    public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.gameinvite.contract.Player";

        public Player()
        {
        }

        public Player(Callback callback)
        {
            this.callback = callback;
        }

        public Player(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Player result);

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
