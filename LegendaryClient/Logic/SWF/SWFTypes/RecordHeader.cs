using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.SWF.SWFTypes
{
    internal class RecordHeader
    {
        private ushort tagCode;
        private uint tagLength;
        private bool longTag;

        public RecordHeader()
        {
        }

        public RecordHeader(int tag, int length)
        {
            tagCode = System.Convert.ToUInt16(tag);
            tagLength = System.Convert.ToUInt32(length);
            if (tagLength > 0x3e)
                longTag = true;
            else
                longTag = false;
        }

        public RecordHeader(bool longTag)
        {
            this.longTag = longTag;
        }

        public RecordHeader(int tag, int length, bool longTag)
        {
            tagCode = System.Convert.ToUInt16(tag);
            tagLength = System.Convert.ToUInt32(length);
            this.longTag = longTag;
        }

        public RecordHeader(int tag, uint length)
        {
            tagCode = System.Convert.ToUInt16(tag);
            tagLength = length;
            if (tagLength > 0x3e)
                longTag = true;
            else
                longTag = false;
        }

        public int TagCode
        {
            get { return this.tagCode; }
            set { this.tagCode = (ushort)value; }
        }

        public uint TagLength
        {
            get { return this.tagLength; }
            set { this.tagLength = value; }
        }

        public void ReadData(BinaryReader binaryReader)
        {
            ushort tagCL = binaryReader.ReadUInt16();
            tagCode = Convert.ToUInt16(tagCL >> 6);
            tagLength = System.Convert.ToUInt32(tagCL - (tagCode << 6));

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
