using System;

namespace PVPNetConnect.RiotObjects.Kudos.Dto
{
    public class PendingKudosDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.kudos.dto.PendingKudosDTO";

        public PendingKudosDTO()
        {
        }

        public PendingKudosDTO(Callback callback)
        {
            this.callback = callback;
        }

        public PendingKudosDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PendingKudosDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("pendingCounts")]
        public Int32[] PendingCounts { get; set; }
    }
}