using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.harassment.HarassmentReport")]
    public class HarassmentReport
    {
        /// <summary>		
        /// The person you're reporting summoner id		
        /// </summary>		
        [SerializedName("reportedSummonerId")]		
        public double SummonerID { get; set; }		
		
        /// <summary>		
        /// The fuck is this?		
        /// </summary>		
        [SerializedName("ipAddress")]		
        public double IPAddress { get; set; }		
		
        /// <summary>		
        /// GameID in lobby		
        /// </summary>		
        [SerializedName("gameId")]		
        public double GameID { get; set; }		
		
        /// <summary>		
        /// Usually "GAME"		
        /// </summary>		
        [SerializedName("reportSource")]		
        public string ReportSource { get; set; }		
		
        /// <summary>		
        /// Comment to send		
        /// </summary>		
        [SerializedName("comment")]		
        public string Comment { get; set; }		
		
        /// <summary>		
        /// Your Summoner ID		
        /// </summary>		
        [SerializedName("reportingSummonerId")]		
        public double ReportingSummonerID { get; set; }		
		
        /// <summary>		
        /// Their offense (E.G. UNSKILLED_PLAYER)		
        /// </summary>		
        [SerializedName("offense")]		
        public string Offense { get; set; }
    }
}
