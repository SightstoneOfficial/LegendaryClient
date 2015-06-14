using RtmpSharp.IO;
using System;

namespace RtmpSharp.Messaging.Messages
{
    [Serializable]
    [SerializedName("flex.messaging.messages.AsyncMessage")]
    public class AsyncMessage : FlexMessage
    {
        [SerializedName("correlationId")]
        public string CorrelationId;

    }

    static class AsyncMessageHeaders
    {
        public const string Subtopic = "DSSubtopic";
        public const string Endpoint = "DSEndpoint";
        public const string ID = "DSId";
    }
}
