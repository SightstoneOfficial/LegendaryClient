using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    public class Invitee : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.gameinvite.contract.Invitee";

        public Invitee()
        {
        }

        public Invitee(Callback callback)
        {
            this.callback = callback;
        }

        public Invitee(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Invitee result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("inviteeStateAsString")]
        public String InviteeState { get; set; }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }

        [InternalName("inviteeState")]
        public String inviteeState { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}