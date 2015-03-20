using PVPNetConnect.RiotObjects.Platform.Reroll.Pojo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class GameParticipant : Participant
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.GameParticipant";

        public GameParticipant()
        {
        }

        public GameParticipant(Callback callback)
        {
            this.callback = callback;
        }

        public GameParticipant(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public new delegate void Callback(GameParticipant result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("pickTurn")]
        public int TimeAddedToQueue { get; set; }

        [InternalName("summonerInternalName")]
        public string SummonerInternalName { get; set; }

        [InternalName("summonerName")]
        public string SummonerName { get; set; }
    }
}
