using System;

namespace PVPNetConnect.RiotObjects.Platform.Reroll.Pojo
{
    public class PointSummary : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.reroll.pojo.PointSummary";

        public PointSummary()
        {
        }

        public PointSummary(Callback callback)
        {
            this.callback = callback;
        }

        public PointSummary(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PointSummary result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("pointsToNextRoll")]
        public Double PointsToNextRoll { get; set; }

        [InternalName("maxRolls")]
        public Int32 MaxRolls { get; set; }

        [InternalName("numberOfRolls")]
        public Int32 NumberOfRolls { get; set; }

        [InternalName("pointsCostToRoll")]
        public Double PointsCostToRoll { get; set; }

        [InternalName("currentPoints")]
        public Double CurrentPoints { get; set; }
    }
}