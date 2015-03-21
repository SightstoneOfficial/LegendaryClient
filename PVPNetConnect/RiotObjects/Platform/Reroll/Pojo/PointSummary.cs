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
        public double PointsToNextRoll { get; set; }

        [InternalName("maxRolls")]
        public int MaxRolls { get; set; }

        [InternalName("numberOfRolls")]
        public int NumberOfRolls { get; set; }

        [InternalName("pointsCostToRoll")]
        public double PointsCostToRoll { get; set; }

        [InternalName("currentPoints")]
        public double CurrentPoints { get; set; }
    }
}