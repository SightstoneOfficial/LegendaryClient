using System.Collections.Generic;

namespace PVPNetConnect
{
    public class Packet
    {
        private byte[] dataBuffer;
        private int dataPos;
        private int dataSize;
        private int packetType;
        private List<byte> rawPacketBytes;

        public Packet()
        {
            this.rawPacketBytes = new List<byte>();
        }

        public void SetSize(int size)
        {
            dataSize = size;
            dataBuffer = new byte[dataSize];
        }

        public void SetType(int type)
        {
            packetType = type;
        }

        public void Add(byte b)
        {
            dataBuffer[dataPos++] = b;
        }

        public bool IsComplete()
        {
            return (dataPos == dataSize);
        }

        public int GetSize()
        {
            return dataSize;
        }

        public int GetPacketType()
        {
            return packetType;
        }

        public byte[] GetData()
        {
            return dataBuffer;
        }

        public void AddToRaw(byte b)
        {
            rawPacketBytes.Add(b);
        }

        public void AddToRaw(byte[] b)
        {
            rawPacketBytes.AddRange(b);
        }

        public byte[] GetRawData()
        {
            return rawPacketBytes.ToArray();
        }
    }
}