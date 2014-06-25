using System;

namespace PVPNetConnect.RiotObjects.Platform.Statistics
{
    public class FellowPlayerInfo : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.statistics.FellowPlayerInfo";

        public FellowPlayerInfo()
        {
        }

        public FellowPlayerInfo(Callback callback)
        {
            this.callback = callback;
        }

        public FellowPlayerInfo(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(FellowPlayerInfo result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("championId")]
        public Double ChampionId { get; set; }

        [InternalName("teamId")]
        public Int32 TeamId { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }
    }
}