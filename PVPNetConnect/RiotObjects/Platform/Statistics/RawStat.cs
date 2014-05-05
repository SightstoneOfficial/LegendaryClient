using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class RawStat : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.RawStat";

        public RawStat()
        {
        }

        public RawStat(Callback callback)
        {
            this.callback = callback;
        }

        public RawStat(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(RawStat result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("statType")]
        public String StatType { get; set; }

        [InternalName("value")]
        public Double Value { get; set; }
    }
}