using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.reroll.pojo.SimpleDialogMessage")]
    public class SimpleDialogMessage
    {
        [SerializedName("titleCode")]
        public String TitleCode { get; set; }

        [SerializedName("accountId")]
        public Double AccountId { get; set; }

        [SerializedName("params")]
        public object Params { get; set; }

        [SerializedName("type")]
        public String Type { get; set; }
    }
}
