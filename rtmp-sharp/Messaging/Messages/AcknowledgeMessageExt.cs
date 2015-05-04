using RtmpSharp.IO;
using RtmpSharp.IO.AMF3;
using System;

namespace RtmpSharp.Messaging.Messages
{
    [Serializable]
    [SerializedName("DSK")]
    public class AcknowledgeMessageExt : AsyncMessageExt, IExternalizable
    {
        public AcknowledgeMessageExt()
        {
        }
        public override void ReadExternal(IDataInput input)
        {
            base.ReadExternal(input);
            var flags = ReadFlags(input);
            for (int i = 0; i < flags.Count; i++)
            {
                ReadRemaining(input, flags[i], 0);
            }
        }

        public override void WriteExternal(IDataOutput output)
        {
            base.WriteExternal(output);
            output.WriteByte(0);

        }
    }
}
