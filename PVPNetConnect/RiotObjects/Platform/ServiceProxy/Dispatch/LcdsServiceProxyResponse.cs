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
        public String Status { get; set; }

        [InternalName("payload")]
        public String Payload { get; set; }

        [InternalName("messageId")]
        public String messageId { get; set; }

        [InternalName("methodName")]
        public String MethodName { get; set; }

        [InternalName("serviceName")]
        public String ServiceName { get; set; }
    }
}
