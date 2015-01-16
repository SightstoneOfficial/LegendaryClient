using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPNetConnect.RiotObjects.Platform.Harassment
{
    public class HarassmentReport : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.harassment.HarassmentReport";

        public HarassmentReport()
        {
        }

        public HarassmentReport(Callback callback)
        {
            this.callback = callback;
        }

        public HarassmentReport(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(HarassmentReport result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        /// <summary>
        /// The person you're reporting summoner id
        /// </summary>
        [InternalName("reportedSummonerId")]
        public Int64 SummonerID { get; set; }

        /// <summary>
        /// The fuck is this?
        /// </summary>
        [InternalName("ipAddress")]
        public Int64 IPAddress { get; set; }

        /// <summary>
        /// GameID in lobby
        /// </summary>
        [InternalName("gameId")]
        public Int64 GameID { get; set; }

        /// <summary>
        /// Usually "GAME"
        /// </summary>
        [InternalName("reportSource")]
        public String ReportSource { get; set; }

        /// <summary>
        /// Comment to send
        /// </summary>
        [InternalName("comment")]
        public String Comment { get; set; }

        /// <summary>
        /// Your Summoner ID
        /// </summary>
        [InternalName("reportingSummonerId")]
        public Int64 ReportingSummonerID { get; set; }

        /// <summary>
        /// Their offense (E.G. UNSKILLED_PLAYER)
        /// </summary>
        [InternalName("offense")]
        public String Offense { get; set; }

    }
}
