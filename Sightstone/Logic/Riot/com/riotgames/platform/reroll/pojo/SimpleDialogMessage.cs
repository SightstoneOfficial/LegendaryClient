using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform.Reroll.Pojo
{
    [Serializable]
    [SerializedName("com.riotgames.platform.reroll.pojo.SimpleDialogMessage")]
    public class SimpleDialogMessage
    {
        [SerializedName("titleCode")]
        public String TitleCode { get; set; }

        [SerializedName("accountId")]
        public double AccountId { get; set; }

        [SerializedName("params")]
        public object Params { get; set; }

        [SerializedName("type")]
        public String Type { get; set; }
    }
}
