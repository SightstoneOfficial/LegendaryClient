using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class RawStatDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.RawStatDTO";

        public RawStatDTO()
        {
        }

        public RawStatDTO(Callback callback)
        {
            this.callback = callback;
        }

        public RawStatDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(RawStatDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("value")]
        public Double Value { get; set; }

        [InternalName("statTypeName")]
        public String StatTypeName { get; set; }

        [InternalName("futureData")]
        public new Boolean FutureData { get; set; }
    }
}