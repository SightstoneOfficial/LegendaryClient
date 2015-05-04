using RtmpSharp.IO;
using RtmpSharp.Messaging.Messages;
using System;

namespace RtmpSharp.Messaging
{
    public class RemotingMessageReceivedEventArgs : EventArgs
    {
        public readonly string Operation;
        public readonly string Destination;
        public readonly string Endpoint;
        public readonly string MessageId;
        public readonly object Body;
        public readonly int InvokeId;
        public bool ReturnRequired;
        public object Data;
        public readonly RemotingMessage OriginalMessage;

        public RemotingMessageReceivedEventArgs(RemotingMessage originalMessage, string operation, string endpoint, string destination, string messageId, object body, int invokeId)
        {
            OriginalMessage = originalMessage;
            Operation = operation;
            Destination = destination;
            Endpoint = endpoint;
            MessageId = messageId;
            Body = body;
            ReturnRequired = false;
            InvokeId = invokeId;
        }
    }
}
