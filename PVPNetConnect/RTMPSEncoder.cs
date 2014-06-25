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

namespace PVPNetConnect
{
    public class RTMPSEncoder
    {
        private long startTime;

        public byte[] AddHeaders(byte[] data)
        {
            List<byte> result = new List<byte>();

            // Header byte
            result.Add((byte)0x03);

            // Timestamp
            long timediff = ((long)DateTime.Now.TimeOfDay.TotalMilliseconds - startTime);
            result.Add((byte)((timediff & 0xFF0000) >> 16));
            result.Add((byte)((timediff & 0x00FF00) >> 8));
            result.Add((byte)(timediff & 0x0000FF));

            // Body size
            result.Add((byte)((data.Length & 0xFF0000) >> 16));
            result.Add((byte)((data.Length & 0x00FF00) >> 8));
            result.Add((byte)(data.Length & 0x0000FF));

            // Content type
            result.Add((byte)0x11);

            // Source ID
            result.Add((byte)0x00);
            result.Add((byte)0x00);
            result.Add((byte)0x00);
            result.Add((byte)0x00);

            // Add body
            for (int i = 0; i < data.Length; i++)
            {
                result.Add(data[i]);
                if (i % 128 == 127 && i != data.Length - 1)
                    result.Add((byte)0xC3);
            }

            byte[] ret = new byte[result.Count];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = result[i];

            return ret;
        }

        public byte[] EncodeConnect(Dictionary<string, object> paramaters)
        {
            startTime = (long)DateTime.Now.TimeOfDay.TotalMilliseconds;
            List<Byte> result = new List<Byte>();

            WriteStringAMF0(result, "connect");
            WriteIntAMF0(result, 1); // invokeId

            // Write params
            result.Add((byte)0x11); // AMF3 object
            result.Add((byte)0x09); // Array
            WriteAssociativeArray(result, paramaters);

            // Write service call args
            result.Add((byte)0x01);
            result.Add((byte)0x00); // false
            WriteStringAMF0(result, "nil"); // "nil"
            WriteStringAMF0(result, ""); // ""

            // Set up CommandMessage
            TypedObject cm = new TypedObject("flex.messaging.messages.CommandMessage");
            cm.Add("operation", 5);
            cm.Add("correlationId", "");
            cm.Add("timestamp", 0);
            cm.Add("messageId", RandomUID());
            cm.Add("body", new TypedObject(null));
            cm.Add("destination", "");
            Dictionary<string, object> headers = new Dictionary<string, object>();
            headers.Add("DSMessagingVersion", 1.0);
            headers.Add("DSId", "my-rtmps");
            cm.Add("headers", headers);
            cm.Add("clientId", null);
            cm.Add("timeToLive", 0);

            // Write CommandMessage
            result.Add((byte)0x11); // AMF3 object
            Encode(result, cm);

            byte[] ret = new byte[result.Count];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = result[i];

            ret = AddHeaders(ret);
            ret[7] = (byte)0x14; // Change message type

            return ret;
        }

        public byte[] EncodeInvoke(int id, object data)
        {
            List<Byte> result = new List<Byte>();

            result.Add((byte)0x00); // version
            result.Add((byte)0x05); // type?
            WriteIntAMF0(result, id); // invoke ID
            result.Add((byte)0x05); // ???

            result.Add((byte)0x11); // AMF3 object
            Encode(result, data);

            byte[] ret = result.ToArray();
            ret = AddHeaders(ret);

            return ret;
        }

        public byte[] EncodeResult(object data, string resultType, int invokeId, int version)
        {
            List<byte> result = new List<byte>();

            result.Add((byte)version); //Version
            WriteStringAMF0(result, resultType); //Type
            WriteIntAMF0(result, invokeId);
            result.Add((byte)0x05); // Service call of null;
            result.Add((byte)0x11); //AMF3 Object
            Encode(result, data);

            byte[] ret = result.ToArray();
            ret = AddHeaders(ret);

            return ret;
        }

        public byte[] Encode(object obj)
        {
            List<byte> result = new List<byte>();
            Encode(result, obj);

            byte[] ret = result.ToArray();

            return ret;
        }

        public void Encode(List<byte> ret, object obj)
        {
            if (obj == null)
            {
                ret.Add((byte)0x01);
            }
            else if (obj is bool)
            {
                bool val = (bool)obj;
                if (val)
                    ret.Add((byte)0x03);
                else
                    ret.Add((byte)0x02);
            }
            else if (obj is int)
            {
                ret.Add((byte)0x04);
                WriteInt(ret, (int)obj);
            }
            else if (obj is double)
            {
                ret.Add((byte)0x05);
                WriteDouble(ret, (double)obj);
            }
            else if (obj is string)
            {
                ret.Add((byte)0x06);
                WriteString(ret, (string)obj);
            }
            else if (obj is DateTime)
            {
                ret.Add((byte)0x08);
                WriteDate(ret, (DateTime)obj);
            }
            // Must precede Object[] check
            else if (obj is byte[])
            {
                ret.Add((byte)0x0C);
                WriteByteArray(ret, (byte[])obj);
            }
            else if (obj is object[])
            {
                ret.Add((byte)0x09);
                WriteArray(ret, (object[])obj);
            }
            // Must precede Map check
            else if (obj is TypedObject)
            {
                ret.Add((byte)0x0A);
                WriteObject(ret, (TypedObject)obj);
            }

            else if (obj is Dictionary<string, object>)
            {
                ret.Add((byte)0x09);
                WriteAssociativeArray(ret, (Dictionary<string, object>)obj);
            }
            else
            {
                throw new Exception("Unexpected object type: " + obj.GetType().FullName);
            }
        }

        private void WriteInt(List<Byte> ret, int val)
        {
            if (val < 0 || val >= 0x200000)
            {
                ret.Add((byte)(((val >> 22) & 0x7f) | 0x80));
                ret.Add((byte)(((val >> 15) & 0x7f) | 0x80));
                ret.Add((byte)(((val >> 8) & 0x7f) | 0x80));
                ret.Add((byte)(val & 0xff));
            }
            else
            {
                if (val >= 0x4000)
                {
                    ret.Add((byte)(((val >> 14) & 0x7f) | 0x80));
                }
                if (val >= 0x80)
                {
                    ret.Add((byte)(((val >> 7) & 0x7f) | 0x80));
                }
                ret.Add((byte)(val & 0x7f));
            }
        }

        private void WriteDouble(List<byte> ret, double val)
        {
            if (Double.IsNaN(val))
            {
                ret.Add((byte)0x7F);
                ret.Add((byte)0xFF);
                ret.Add((byte)0xFF);
                ret.Add((byte)0xFF);
                ret.Add((byte)0xE0);
                ret.Add((byte)0x00);
                ret.Add((byte)0x00);
                ret.Add((byte)0x00);
            }
            else
            {
                byte[] temp = BitConverter.GetBytes((double)val);

                for (int i = temp.Length - 1; i >= 0; i--)
                    ret.Add(temp[i]);
            }
        }

        private void WriteString(List<byte> ret, string val)
        {
            byte[] temp = null;
            try
            {
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                temp = encoding.GetBytes(val);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to encode string as UTF-8: " + val + '\n' + e.Message);
            }

            WriteInt(ret, (temp.Length << 1) | 1);

            foreach (byte b in temp)
                ret.Add(b);
        }

        private void WriteDate(List<Byte> ret, DateTime val)
        {
            ret.Add((byte)0x01);
            WriteDouble(ret, (double)val.TimeOfDay.TotalMilliseconds);
        }

        private void WriteArray(List<byte> ret, object[] val)
        {
            WriteInt(ret, (val.Length << 1) | 1);
            ret.Add((byte)0x01);
            foreach (object obj in val)
                Encode(ret, obj);
        }

        private void WriteAssociativeArray(List<Byte> ret, Dictionary<string, object> val)
        {
            ret.Add((byte)0x01);
            foreach (string key in val.Keys)
            {
                WriteString(ret, key);
                Encode(ret, val[key]);
            }
            ret.Add((byte)0x01);
        }

        private void WriteObject(List<byte> ret, TypedObject val)
        {
            if (val.type == null || val.type.Equals(""))
            {
                ret.Add((byte)0x0B); // Dynamic class

                ret.Add((byte)0x01); // No class name
                foreach (string key in val.Keys)
                {
                    WriteString(ret, key);
                    Encode(ret, val[key]);
                }
                ret.Add((byte)0x01); // End of dynamic
            }
            else if (val.type.Equals("flex.messaging.io.ArrayCollection"))
            {
                ret.Add((byte)0x07); // Externalizable
                WriteString(ret, val.type);

                Encode(ret, val["array"]);
            }
            else if (val.type.Equals("DSK"))
            {
                WriteInt(ret, (val.Count << 4) | 3); // Inline + member count
                WriteString(ret, val.type);
                byte[] flag1 = new byte[8];
                byte[] flag2 = new byte[8];
                if (val["body"] != null)
                    flag1[0] = 1;
                if (val["clientId"] != null)
                    flag1[1] = 1;
                if (val["destination"] != null)
                    flag1[2] = 1;
                if (val["headers"] != null)
                    flag1[3] = 1;
                if (val["messageId"] != null && val["clientId"] != null)
                    flag1[4] = 1;
                if (val["timestamp"] != null && val["destination"] != null)
                    flag1[5] = 1;
                if (val["timeToLive"] != null && val["headers"] != null)
                    flag1[6] = 1;

                if (val["clientIdBytes"] != null)
                    flag2[0] = 1;
                if (val["messageIdBytes"] != null)
                    flag2[1] = 1;

                WriteObject(ret, val.GetTO("data"));
            }
            else
            {
                WriteInt(ret, (val.Count << 4) | 3); // Inline + member count
                WriteString(ret, val.type);

                List<String> keyOrder = new List<String>();
                foreach (string key in val.Keys)
                {
                    WriteString(ret, key);
                    keyOrder.Add(key);
                }

                foreach (string key in keyOrder)
                    Encode(ret, val[key]);
            }
        }

        private void WriteByteArray(List<byte> ret, byte[] val)
        {
            throw new NotImplementedException("Encoding byte arrays is not implemented");
        }

        private void WriteIntAMF0(List<byte> ret, int val)
        {
            ret.Add((byte)0x00);

            byte[] temp = BitConverter.GetBytes((double)val);

            for (int i = temp.Length - 1; i >= 0; i--)
                ret.Add(temp[i]);
            //foreach (byte b in temp)
            //ret.Add(b);
        }

        private void WriteStringAMF0(List<byte> ret, string val)
        {
            byte[] temp = null;
            try
            {
                System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                temp = encoding.GetBytes(val);
            }
            catch (Exception e)
            {
                throw new Exception("Unable to encode string as UTF-8: " + val + '\n' + e.Message);
            }

            ret.Add((byte)0x02);

            ret.Add((byte)((temp.Length & 0xFF00) >> 8));
            ret.Add((byte)(temp.Length & 0x00FF));

            foreach (byte b in temp)
                ret.Add(b);
        }

        public static string RandomUID()
        {
            Random rand = new Random();

            byte[] bytes = new byte[16];
            rand.NextBytes(bytes);

            StringBuilder ret = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                    ret.Append('-');
                ret.AppendFormat("{0:X2}", bytes[i]);
            }

            return ret.ToString();
        }
    }
}