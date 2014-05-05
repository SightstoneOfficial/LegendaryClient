using System;

namespace PVPNetConnect.RiotObjects.Leagues.Pojo
{
    public class MiniSeriesDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.leagues.pojo.MiniSeriesDTO";

        public MiniSeriesDTO()
        {
        }

        public MiniSeriesDTO(Callback callback)
        {
            this.callback = callback;
        }

        public MiniSeriesDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(MiniSeriesDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("progress")]
        public String Progress { get; set; }

        [InternalName("target")]
        public Int32 Target { get; set; }

        [InternalName("losses")]
        public Int32 Losses { get; set; }

        [InternalName("timeLeftToPlayMillis")]
        public Double TimeLeftToPlayMillis { get; set; }

        [InternalName("wins")]
        public Int32 Wins { get; set; }
    }
}