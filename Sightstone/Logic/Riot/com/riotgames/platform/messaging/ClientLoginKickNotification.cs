using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.messaging.ClientLoginKickNotification")]
    public class ClientLoginKickNotification
    {
        [SerializedName("sessionToken")]		
        public string sessionToken { get; set; }		
    }
}
