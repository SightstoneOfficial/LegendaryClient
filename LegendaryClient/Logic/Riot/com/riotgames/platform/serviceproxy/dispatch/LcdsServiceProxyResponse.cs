using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.serviceproxy.dispatch.LcdsServiceProxyResponse")]
    public class LcdsServiceProxyResponse
    {
        [SerializedName("status")]
        public string Status { get; set; }

        [SerializedName("payload")]
        public string Payload { get; set; }

        [SerializedName("messageId")]
        public string messageId { get; set; }

        [SerializedName("methodName")]
        public string MethodName { get; set; }

        [SerializedName("serviceName")]
        public string ServiceName { get; set; }
    }
}
