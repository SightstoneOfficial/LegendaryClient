#region

using System;
using System.IO;

#endregion

namespace LegendaryClient.Logic.SWF.SWFTypes
{
    internal class RecordHeader
    {
        private bool longTag;
        private ushort tagCode;
        private uint tagLength;

        public RecordHeader()
        {
        }

        public RecordHeader(int tag, int length)
        {
            tagCode = Convert.ToUInt16(tag);
            tagLength = Convert.ToUInt32(length);

            longTag = tagLength > 0x3e;
        }

        public RecordHeader(bool longTag)
        {
            this.longTag = longTag;
        }

        public RecordHeader(int tag, int length, bool longTag)
        {
            tagCode = Convert.ToUInt16(tag);
            tagLength = Convert.ToUInt32(length);
            this.longTag = longTag;
        }

        public RecordHeader(int tag, uint length)
        {
            tagCode = Convert.ToUInt16(tag);
            tagLength = length;

            longTag = tagLength > 0x3e;
        }

        public int TagCode
        {
            get { return tagCode; }
            set { tagCode = (ushort) value; }
        }

        public uint TagLength
        {
            get { return tagLength; }
            set { tagLength = value; }
        }

        public void ReadData(BinaryReader binaryReader)
        {
            ushort tagCL = binaryReader.ReadUInt16();
            tagCode = Convert.ToUInt16(tagCL >> 6);
            tagLength = Convert.ToUInt32(tagCL - (tagCode << 6));

            bool longTag;

            if (tagLength == 0x3F)
            {
                uint len = binaryReader.ReadUInt32();
                tagLength = len;
                longTag = (tagLength <= 127);
            }
            else
            {
                longTag = false;
            }

            if (tagLength > binaryReader.BaseStream.Length)
            {
                throw new Exception("Invalid Tag Length");
            }
        }
    }
}