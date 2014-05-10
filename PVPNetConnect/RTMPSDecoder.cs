/**
 * A very basic RTMPS client
 *
 * @author Gabriel Van Eyck
 */
/////////////////////////////////////////////////////////////////////////////////
//
//Ported to C# by Ryan A. LaSarre
//
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace PVPNetConnect
{
    public class RTMPSDecoder
    {
        // Stores the data to be consumed while decoding
        private byte[] dataBuffer;

        private int dataPos;

        // Lists of references and class definitions seen so far
        private List<string> stringReferences = new List<string>();

        private List<object> objectReferences = new List<object>();
        private List<ClassDefinition> classDefinitions = new List<ClassDefinition>();

        private void Reset()
        {
            stringReferences.Clear();
            objectReferences.Clear();
            classDefinitions.Clear();
        }

        public TypedObject DecodeConnect(byte[] data)
        {
            Reset();

            dataBuffer = data;
            dataPos = 0;

            TypedObject result = new TypedObject("Invoke");
            result.Add("result", DecodeAMF0());
            result.Add("invokeId", DecodeAMF0());
            result.Add("serviceCall", DecodeAMF0());
            result.Add("data", DecodeAMF0());
            if (dataPos != dataBuffer.Length)
            {
                for (int i = dataPos; i < data.Length; i++)
                {
                    if (ReadByte() != '\0')
                        throw new Exception("There is other data in the buffer!");
                }
            }
            if (dataPos != dataBuffer.Length)
                throw new Exception("Did not consume entire buffer: " + dataPos + " of " + dataBuffer.Length);

            return result;
        }

        public TypedObject DecodeInvoke(byte[] data)
        {
            Reset();

            dataBuffer = data;
            dataPos = 0;

            TypedObject result = new TypedObject("Invoke");
            if (dataBuffer[0] == 0x00)
            {
                dataPos++;
                result.Add("version", 0x00);
            }
            result.Add("result", DecodeAMF0());
            result.Add("invokeId", DecodeAMF0());
            result.Add("serviceCall", DecodeAMF0());
            result.Add("data", DecodeAMF0());

            if (dataPos != dataBuffer.Length)
                throw new Exception("Did not consume entire buffer: " + dataPos + " of " + dataBuffer.Length);

            string[] typeNames = new string[classDefinitions.Count];
            for (int i = 0; i < classDefinitions.Count; i++)
            {
                typeNames[i] = classDefinitions[i].type;
            }

            return result;
        }

        public object Decode(byte[] data)
        {
            dataBuffer = data;
            dataPos = 0;

            object result = Decode();

            if (dataPos != dataBuffer.Length)
                throw new Exception("Did not consume entire buffer: " + dataPos + " of " + dataBuffer.Length);

            return result;
        }

        private object Decode()
        {
            byte type = ReadByte();
            switch (type)
            {
                case 0x00:
                    throw new Exception("Undefined data type");

                case 0x01:
                    return null;

                case 0x02:
                    return false;

                case 0x03:
                    return true;

                case 0x04:
                    return ReadInt();

                case 0x05:
                    return ReadDouble();

                case 0x06:
                    return ReadString();

                case 0x07:
                    return ReadXML();

                case 0x08:
                    return ReadDate();

                case 0x09:
                    return ReadArray();

                case 0x0A:
                    return ReadObject();

                case 0x0B:
                    return ReadXMLString();

                case 0x0C:
                    return ReadByteArray();
            }

            throw new Exception("Unexpected AMF3 data type: " + type);
        }

        private byte ReadByte()
        {
            byte ret = dataBuffer[dataPos];
            dataPos++;
            return ret;
        }

        private int ReadByteAsInt()
        {
            int ret = ReadByte();
            if (ret < 0)
                ret += 256;
            return ret;
        }

        private byte[] ReadBytes(int length)
        {
            byte[] ret = new byte[length];
            for (int i = 0; i < length; i++)
            {
                ret[i] = dataBuffer[dataPos];
                dataPos++;
            }
            return ret;
        }

        private int ReadInt()
        {
            int ret = ReadByteAsInt();
            int tmp;

            if (ret < 128)
            {
                return ret;
            }
            else
            {
                ret = (ret & 0x7f) << 7;
                tmp = ReadByteAsInt();
                if (tmp < 128)
                {
                    ret = ret | tmp;
                }
                else
                {
                    ret = (ret | tmp & 0x7f) << 7;
                    tmp = ReadByteAsInt();
                    if (tmp < 128)
                    {
                        ret = ret | tmp;
                    }
                    else
                    {
                        ret = (ret | tmp & 0x7f) << 8;
                        tmp = ReadByteAsInt();
                        ret = ret | tmp;
                    }
                }
            }

            // Sign extend
            int mask = 1 << 28;
            int r = -(ret & mask) | ret;
            return r;
        }

        private double ReadDouble()
        {
            long value = 0;
            for (int i = 0; i < 8; i++)
                value = (value << 8) + ReadByteAsInt();

            return BitConverter.Int64BitsToDouble(value);
        }

        private string ReadString()
        {
            int handle = ReadInt();
            bool inline = ((handle & 1) != 0);
            handle = handle >> 1;

            if (inline)
            {
                if (handle == 0)
                    return "";

                byte[] data = ReadBytes(handle);

                string str;
                try
                {
                    System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                    str = enc.GetString(data);
                }
                catch (Exception e)
                {
                    throw new Exception("Error parsing AMF3 string from " + data + '\n' + e.Message);
                }

                stringReferences.Add(str);

                return str;
            }
            else
            {
                return stringReferences[handle];
            }
        }

        private string ReadXML()
        {
            throw new NotImplementedException("Reading of XML is not implemented");
        }

        private DateTime ReadDate()
        {
            int handle = ReadInt();
            bool inline = ((handle & 1) != 0);
            handle = handle >> 1;

            if (inline)
            {
                long ms = (long)ReadDouble();
                DateTime d = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                d = d.AddSeconds(ms / 1000);

                objectReferences.Add(d);

                return d;
            }
            else
            {
                return DateTime.MinValue;
            }
        }

        private object[] ReadArray()
        {
            int handle = ReadInt();
            bool inline = ((handle & 1) != 0);
            handle = handle >> 1;

            if (inline)
            {
                string key = ReadString();
                if (key != null && !key.Equals(""))
                    throw new NotImplementedException("Associative arrays are not supported");

                object[] ret = new object[handle];
                objectReferences.Add(ret);

                for (int i = 0; i < handle; i++)
                    ret[i] = Decode();

                return ret;
            }
            else
            {
                return (object[])objectReferences[handle];
            }
        }

        private List<object> ReadList()
        {
            int handle = ReadInt();
            bool inline = ((handle & 1) != 0);
            handle = handle >> 1;

            if (inline)
            {
                string key = ReadString();
                if (key != null && !key.Equals(""))
                    throw new NotImplementedException("Associative arrays are not supported");

                List<object> ret = new List<object>();
                objectReferences.Add(ret);

                for (int i = 0; i < handle; i++)
                    ret.Add(Decode());

                return ret;
            }
            else
            {
                return (List<object>)objectReferences[handle];
            }
        }

        private object ReadObject()
        {
            int handle = ReadInt();
            bool inline = ((handle & 1) != 0);
            handle = handle >> 1;

            if (inline)
            {
                bool inlineDefine = ((handle & 1) != 0);
                handle = handle >> 1;

                ClassDefinition cd;
                if (inlineDefine)
                {
                    cd = new ClassDefinition();
                    cd.type = ReadString();

                    cd.externalizable = ((handle & 1) != 0);
                    handle = handle >> 1;
                    cd.dynamic = ((handle & 1) != 0);
                    handle = handle >> 1;

                    for (int i = 0; i < handle; i++)
                        cd.members.Add(ReadString());

                    classDefinitions.Add(cd);
                }
                else
                {
                    cd = classDefinitions[handle];
                }

                TypedObject ret = new TypedObject(cd.type);

                // Need to add reference here due to circular references
                objectReferences.Add(ret);

                if (cd.externalizable)
                {
                    if (cd.type.Equals("DSK"))
                        ret = ReadDSK();
                    else if (cd.type.Equals("DSA"))
                        ret = ReadDSA();
                    else if (cd.type.Equals("flex.messaging.io.ArrayCollection"))
                    {
                        object obj = Decode();
                        ret = TypedObject.MakeArrayCollection((object[])obj);
                    }
                    else if (cd.type.Equals("com.riotgames.platform.systemstate.ClientSystemStatesNotification") ||
                             cd.type.Equals("com.riotgames.platform.broadcast.BroadcastNotification"))
                    {
                        int size = 0;
                        for (int i = 0; i < 4; i++)
                            size = size * 256 + ReadByteAsInt();

                        byte[] data = ReadBytes(size);
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < data.Length; i++)
                            sb.Append(Convert.ToChar(data[i]));

                        JavaScriptSerializer serializer = new JavaScriptSerializer();
                        ret = serializer.Deserialize<TypedObject>(sb.ToString());
                        ret.type = cd.type;
                    }
                    else
                    {
                        //for (int i = dataPos; i < dataBuffer.length; i++)
                        //System.out.print(String.format("%02X", dataBuffer[i]));
                        //System.out.println();
                        throw new NotImplementedException("Externalizable not handled for " + cd.type);
                    }
                }
                else
                {
                    for (int i = 0; i < cd.members.Count; i++)
                    {
                        String key = cd.members[i];
                        object value = Decode();
                        ret.Add(key, value);
                    }

                    if (cd.dynamic)
                    {
                        String key;
                        while ((key = ReadString()).Length != 0)
                        {
                            object value = Decode();
                            ret.Add(key, value);
                        }
                    }
                }

                return ret;
            }
            else
            {
                return objectReferences[handle];
            }
        }

        private string ReadXMLString()
        {
            throw new NotImplementedException("Reading of XML strings is not implemented");
        }

        private byte[] ReadByteArray()
        {
            int handle = ReadInt();
            bool inline = ((handle & 1) != 0);
            handle = handle >> 1;

            if (inline)
            {
                byte[] ret = ReadBytes(handle);
                objectReferences.Add(ret);
                return ret;
            }
            else
            {
                return (byte[])objectReferences[handle];
            }
        }

        private TypedObject ReadDSA()
        {
            TypedObject ret = new TypedObject("DSA");

            int flag;
            List<int> flags = ReadFlags();
            for (int i = 0; i < flags.Count; i++)
            {
                flag = flags[i];
                int bits = 0;
                if (i == 0)
                {
                    if ((flag & 0x01) != 0)
                        ret.Add("body", Decode());
                    if ((flag & 0x02) != 0)
                        ret.Add("clientId", Decode());
                    if ((flag & 0x04) != 0)
                        ret.Add("destination", Decode());
                    if ((flag & 0x08) != 0)
                        ret.Add("headers", Decode());
                    if ((flag & 0x10) != 0)
                        ret.Add("messageId", Decode());
                    if ((flag & 0x20) != 0)
                        ret.Add("timeStamp", Decode());
                    if ((flag & 0x40) != 0)
                        ret.Add("timeToLive", Decode());
                    bits = 7;
                }
                else if (i == 1)
                {
                    if ((flag & 0x01) != 0)
                    {
                        ReadByte();
                        byte[] temp = ReadByteArray();
                        ret.Add("clientIdBytes", temp);
                        ret.Add("clientId", ByteArrayToID(temp));
                    }
                    if ((flag & 0x02) != 0)
                    {
                        ReadByte();
                        byte[] temp = ReadByteArray();
                        ret.Add("messageIdBytes", temp);
                        ret.Add("messageId", ByteArrayToID(temp));
                    }
                    bits = 2;
                }

                ReadRemaining(flag, bits);
            }

            flags = ReadFlags();
            for (int i = 0; i < flags.Count; i++)
            {
                flag = flags[i];
                int bits = 0;

                if (i == 0)
                {
                    if ((flag & 0x01) != 0)
                        ret.Add("correlationId", Decode());
                    if ((flag & 0x02) != 0)
                    {
                        ReadByte();
                        byte[] temp = ReadByteArray();
                        ret.Add("correlationIdBytes", temp);
                        ret.Add("correlationId", ByteArrayToID(temp));
                    }
                    bits = 2;
                }

                ReadRemaining(flag, bits);
            }

            return ret;
        }

        private TypedObject ReadDSK()
        {
            // DSK is just a DSA + extra set of flags/objects
            TypedObject ret = ReadDSA();
            ret.type = "DSK";

            List<int> flags = ReadFlags();
            for (int i = 0; i < flags.Count; i++)
                ReadRemaining(flags[i], 0);

            return ret;
        }

        private List<int> ReadFlags()
        {
            List<int> flags = new List<int>();
            int flag;
            do
            {
                flag = ReadByteAsInt();
                flags.Add(flag);
            } while ((flag & 0x80) != 0);

            return flags;
        }

        private void ReadRemaining(int flag, int bits)
        {
            // For forwards compatibility, read in any other flagged objects to
            // preserve the integrity of the input stream...
            if ((flag >> bits) != 0)
            {
                for (int o = bits; o < 6; o++)
                {
                    if (((flag >> o) & 1) != 0)
                        Decode();
                }
            }
        }

        private string ByteArrayToID(byte[] data)
        {
            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                    ret.Append('-');
                ret.AppendFormat("{0:X2}", data[i]);
            }

            return ret.ToString();
        }

        private object DecodeAMF0()
        {
            int type = ReadByte();
            switch (type)
            {
                case 0x00:
                    return ReadIntAMF0();

                case 0x02:
                    return ReadStringAMF0();

                case 0x03:
                    return ReadObjectAMF0();

                case 0x05:
                    return null;

                case 0x11: // AMF3
                    return Decode();
            }

            throw new NotImplementedException("AMF0 type not supported: " + type);
        }

        private string ReadStringAMF0()
        {
            int length = (ReadByteAsInt() << 8) + ReadByteAsInt();
            if (length == 0)
                return "";

            byte[] data = ReadBytes(length);

            // UTF-8 applicable?
            string str;
            try
            {
                System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
                str = enc.GetString(data);
            }
            catch (Exception e)
            {
                throw new Exception("Error parsing AMF0 string from " + data + '\n' + e.Message);
            }

            return str;
        }

        private int ReadIntAMF0()
        {
            return (int)ReadDouble();
        }

        private TypedObject ReadObjectAMF0()
        {
            TypedObject body = new TypedObject("Body");
            string key;
            while (!(key = ReadStringAMF0()).Equals(""))
            {
                byte b = ReadByte();
                if (b == 0x00)
                    body.Add(key, ReadDouble());
                else if (b == 0x02)
                    body.Add(key, ReadStringAMF0());
                else if (b == 0x05)
                    body.Add(key, null);
                else
                    throw new NotImplementedException("AMF0 type not supported: " + b);
            }
            ReadByte(); // Skip object end marker

            return body;
        }
    }
}