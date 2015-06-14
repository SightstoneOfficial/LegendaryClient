using RtmpSharp.IO;

namespace RtmpSharp.Messaging.Messages
{
    public class FlexMessage
    {
        [SerializedName("clientId")]
        public string ClientId { get; set; }

        [SerializedName("destination")]
        public string Destination { get; set; }

        [SerializedName("messageId")]
        public string MessageId { get; set; }

        [SerializedName("timestamp")]
        public long Timestamp { get; set; }

        // TTL (in milliseconds) that message is valid for after `Timestamp`
        [SerializedName("timeToLive")]
        public long TimeToLive { get; set; }

        [SerializedName("body")]
        public object Body { get; set; }

        [SerializedName("headers")]
        public AsObject Headers
        {
            get { return headers; }
            set { headers = value; }
        }
        AsObject headers;

        public FlexMessage()
        {
            MessageId = Uuid.NewUuid();
        }
    }

    static class FlexMessageHeaders
    {
        // Messages pushed from the server may arrive in a batch, with messages in the batch 
        // potentially targeted to different Consumer instances.
        // Each message will contain this header identifying the Consumer instance that will 
        // receive the message.
        public const string DestinationClientId = "DSDstClientId";
        // Messages are tagged with the endpoint id for the Channel they are sent over.
        // Channels set this value automatically when they send a message.
        public const string Endpoint = "DSEndpoint";
        // Messages that need to set remote credentials for a destination carry the Base64 encoded 
        // credentials in this header.
        public const string RemoteCredentials = "DSRemoteCredentials";
        // Messages sent with a defined request timeout use this header.
        // The request timeout value is set on outbound messages by services or channels and the value 
        // controls how long the corresponding MessageResponder will wait for an acknowledgement, 
        // result or fault response for the message before timing out the request.
        public const string RequestTimeout = "DSRequestTimeout";
        // This header is used to transport the global FlexClient Id value in outbound messages 
        // once it has been assigned by the server.
        public const string FlexClientId = "DSId";
        public const string FlexSubtopic = "DSSubtopic";
        public const string FlexMessagingVersion = "DSMessagingVersion";
    }
}
