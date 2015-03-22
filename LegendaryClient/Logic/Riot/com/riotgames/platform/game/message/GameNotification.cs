using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.message.GameNotification")]
    public class GameNotification
    {
        [SerializedName("messageCode")]
        public String MessageCode { get; set; }

        [SerializedName("type")]
        public String Type { get; set; }

        [SerializedName("messageArgument")]
        public object MessageArgument { get; set; }
    }
}
