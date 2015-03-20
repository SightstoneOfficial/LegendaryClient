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
        public double Value { get; set; }

        [InternalName("statTypeName")]
        public string StatTypeName { get; set; }

        [InternalName("futureData")]
        public new bool FutureData { get; set; }
    }
}