using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.messaging.ClientLoginKickNotification")]
    public class ClientLoginKickNotification
    {
        [SerializedName("sessionToken")]		
        public string sessionToken { get; set; }		
    }
}
