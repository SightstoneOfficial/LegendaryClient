using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract
{
    public class LobbyStatus : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.gameinvite.contract.LobbyStatus";

        public LobbyStatus()
        {
        }

        public LobbyStatus(Callback callback)
        {
            this.callback = callback;
        }

        public LobbyStatus(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(LobbyStatus result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("chatKey")]
        public String ChatKey { get; set; }

        [InternalName("gameMetaData")]
        public String GameData { get; set; }

        [InternalName("owner")]
        public Player Owner { get; set; }
        //public Player Owner { get; set; }

        [InternalName("members")]
        public List<Member> Members { get; set; }

        [InternalName("invitees")]
        public List<Invitee> Invitees { get; set; }

        [InternalName("invitationId")]
        public String InvitationID { get; set; }
    }
}