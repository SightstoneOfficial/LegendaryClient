using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVPNetConnect.RiotObjects.Platform.ServiceProxy.Dispatch
{
    public class LcdsServiceProxyResponse : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.serviceproxy.dispatch.LcdsServiceProxyResponse";

        public LcdsServiceProxyResponse()
        {
        }

        public LcdsServiceProxyResponse(Callback callback)
        {
            this.callback = callback;
        }

        public LcdsServiceProxyResponse(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(LcdsServiceProxyResponse result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("status")]
        public string Status { get; set; }

        [InternalName("payload")]
        public string Payload { get; set; }

        [InternalName("messageId")]
        public string messageId { get; set; }

        [InternalName("methodName")]
        public string MethodName { get; set; }

        [InternalName("serviceName")]
        public string ServiceName { get; set; }
    }
}
