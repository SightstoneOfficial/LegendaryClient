using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class TimeTrackedStat : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.TimeTrackedStat";

        public TimeTrackedStat()
        {
        }

        public TimeTrackedStat(Callback callback)
        {
            this.callback = callback;
        }

        public TimeTrackedStat(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TimeTrackedStat result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("timestamp")]
        public DateTime Timestamp { get; set; }

        [InternalName("type")]
        public String Type { get; set; }
    }
}