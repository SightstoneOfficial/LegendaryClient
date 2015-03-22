using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.messaging.StoreFulfillmentNotification")]
    public class StoreFulfillmentNotification
    {
        [SerializedName("rp")]
        public Double Rp { get; set; }

        [SerializedName("ip")]
        public Double Ip { get; set; }

        [SerializedName("inventoryType")]
        public String InventoryType { get; set; }

        [SerializedName("data")]
        public object Data { get; set; }
    }
}
