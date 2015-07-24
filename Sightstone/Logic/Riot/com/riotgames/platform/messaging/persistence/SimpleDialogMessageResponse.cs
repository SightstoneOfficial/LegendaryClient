using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.messaging.persistence.SimpleDialogMessageResponse")]
    public class SimpleDialogMessageResponse
    {
        [SerializedName("command")]		
        public string Command { get; set; }		
		
        [SerializedName("accountId")]		
        public double AccountId { get; set; }		
		
        [SerializedName("msgId")]		
        public double MessageId { get; set; }
    }
}
