using RtmpSharp.IO;
using RtmpSharp.IO.AMF3;
using System;
using System.Collections.Generic;

namespace RtmpSharp.Messaging.Messages
{
    [Serializable]
    [SerializedName("DSA")]
    public class AsyncMessageExt : AsyncMessage, IExternalizable
    {
        public AsyncMessageExt()
        {
            Timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        [SerializedName("correlationIdBytes")]
        public ByteArray CorrelationIdBytes
        {
            get { return Uuid.ToBytes(CorrelationId); }
            set { CorrelationId = Uuid.ToString(value); }
        }

        [SerializedName("clientIdBytes")]
        public ByteArray ClientIdBytes
        {
            get { return Uuid.ToBytes(ClientId); }
            set { ClientId = Uuid.ToString(value); }
        }

        [SerializedName("messageIdBytes")]
        public ByteArray MessageIdBytes
        {
            get { return Uuid.ToBytes(MessageId); }
            set { MessageId = Uuid.ToString(value); }
        }

        public virtual void ReadExternal(IDataInput input)
        {

            List<byte> flags = ReadFlags(input);
            int bits = 0;
            for (int i = 0; i < flags.Count; i++)
            {
                byte flag = flags[i];
                if (i == 0)
                {
                    if ((flag & 0x01) != 0)
                        Body = input.ReadObject();
                    if ((flag & 0x02) != 0)
                        ClientId = (string)input.ReadObject();
                    if ((flag & 0x04) != 0)
                        Destination = (string)input.ReadObject();
                    if ((flag & 0x08) != 0)
                        Headers = input.ReadObject() as AsObject;
                    if ((flag & 0x10) != 0)
                        MessageId = (string)input.ReadObject();
                    if ((flag & 0x20) != 0)
                        Timestamp = Convert.ToInt64(input.ReadObject());
                    if ((flag & 0x40) != 0)
                        TimeToLive = Convert.ToInt64(input.ReadObject());
                    bits = 7;
                }
                else if (i == 1)
                {
                    if ((flag & 0x01) != 0)
                    {
                        ClientIdBytes = ((ByteArray)input.ReadObject());
                    }
                    if ((flag & 0x02) != 0)
                    {
                        MessageIdBytes = ((ByteArray)input.ReadObject());
                    }
                    bits = 2;
                }
                ReadRemaining(input, flag, bits);
            }

            flags = ReadFlags(input);
            for (int i = 0; i < flags.Count; i++)
            {
                byte flag = flags[i];
                bits = 0;

                if (i == 0)
                {
                    if ((flag & 0x01) != 0)
                        CorrelationId = (string)input.ReadObject();
                    if ((flag & 0x02) != 0)
                    {
                        CorrelationIdBytes = ((ByteArray)input.ReadObject());
                    }
                    bits = 2;
                }

                ReadRemaining(input, flag, bits);
            }
        }

        public virtual void WriteExternal(IDataOutput output)
        {
            byte f1 = 0;
            if (Body != null)
                f1 |= 0x01;
            if (ClientId != null && ClientIdBytes == null)
                f1 |= 0x02;
            if (Destination != null)
                f1 |= 0x04;
            if (Headers != null)
                f1 |= 0x08;
            if (MessageId != null && MessageIdBytes == null)
                f1 |= 0x10;
            if (Timestamp != 0)
                f1 |= 0x20;
            if (TimeToLive != 0)
                f1 |= 0x40;

            byte f2 = 0;

            if (ClientIdBytes != null)
                f2 |= 0x01;
            if (MessageIdBytes != null)
                f2 |= 0x02;

            if (f2 != 0) f1 |= 0x80;

            output.WriteByte(f1);
            if ((f1&0x80) != 0) output.WriteByte(f2);


            if (Body != null)
                output.WriteObject(Body);
            if (ClientId != null && ClientIdBytes == null)
                output.WriteObject(ClientId);
            if (Destination != null)
                output.WriteObject(Destination);
            if (Headers != null)
                output.WriteObject(Headers);
            if (MessageId != null && MessageIdBytes == null)
                output.WriteObject(MessageId);
            if (Timestamp != 0)
                output.WriteObject(Timestamp);
            if (TimeToLive != 0)
                output.WriteObject(TimeToLive);

            if (ClientIdBytes != null)
                output.WriteObject(ClientIdBytes);
            if (MessageIdBytes != null)
                output.WriteObject(MessageIdBytes);

            byte f3 = 0;
            if (CorrelationId != null && CorrelationIdBytes == null)
                f3 |= 0x01;
            if (CorrelationIdBytes != null)
                f3 |= 0x02;

            output.WriteByte(f3);

            if (CorrelationId != null && CorrelationIdBytes == null)
                output.WriteObject(CorrelationId);
            if (CorrelationIdBytes != null)
                output.WriteObject(CorrelationIdBytes);


        }

        protected List<byte> ReadFlags(IDataInput input)
        {
            List<byte> flags = new List<byte>();
            byte flag;
            do
            {
                flag = input.ReadByte();
                flags.Add(flag);
            } while ((flag & 0x80) != 0);

            return flags;
        }

        protected void ReadRemaining(IDataInput input, int flag, int bits)
        {
            // For forwards compatibility, read in any other flagged objects to
            // preserve the integrity of the input stream...
            if ((flag >> bits) != 0)
            {
                for (int o = bits; o < 6; o++)
                {
                    if (((flag >> o) & 1) != 0)
                        input.ReadObject();
                }
            }
        }
    }
}
