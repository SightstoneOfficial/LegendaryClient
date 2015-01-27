using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    public class Member : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.gameinvite.contract.Member";

        public Member()
        {
        }

        public Member(Callback callback)
        {
            this.callback = callback;
        }

        public Member(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Member result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("hasDelegatedInvitePower")]
        public bool hasDelegatedInvitePower { get; set; }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}