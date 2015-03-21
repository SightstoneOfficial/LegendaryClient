using System;

namespace PVPNetConnect.RiotObjects.Platform.Reroll.Pojo
{
    internal class EogPointChangeBreakdown : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.reroll.pojo.EogPointChangeBreakdown";

        public EogPointChangeBreakdown()
        {
        }

        public EogPointChangeBreakdown(Callback callback)
        {
            this.callback = callback;
        }

        public EogPointChangeBreakdown(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(EogPointChangeBreakdown result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("pointChangeFromGamePlay")]
        public double PointChangeFromGamePlay { get; set; }

        [InternalName("pointChangeFromChampionsOwned")]
        public double PointChangeFromChampionsOwned { get; set; }

        [InternalName("previousPoints")]
        public double PreviousPoints { get; set; }

        [InternalName("pointsUsed")]
        public double PointsUsed { get; set; }

        [InternalName("endPoints")]
        public double EndPoints { get; set; }
    }
}