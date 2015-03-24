using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.reroll.pojo.StoreAccountBalanceNotification")]
    public class StoreAccountBalanceNotification
    {
        [SerializedName("rp")]
        public double Rp { get; set; }

        [SerializedName("ip")]
        public double Ip { get; set; }
    }
}
