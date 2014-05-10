using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class TalentRow : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.TalentRow";

        public TalentRow()
        {
        }

        public TalentRow(Callback callback)
        {
            this.callback = callback;
        }

        public TalentRow(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(TalentRow result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("index")]
        public Int32 Index { get; set; }

        [InternalName("talents")]
        public List<Talent> Talents { get; set; }

        [InternalName("tltGroupId")]
        public Int32 TltGroupId { get; set; }

        [InternalName("pointsToActivate")]
        public Int32 PointsToActivate { get; set; }

        [InternalName("tltRowId")]
        public Int32 TltRowId { get; set; }
    }
}