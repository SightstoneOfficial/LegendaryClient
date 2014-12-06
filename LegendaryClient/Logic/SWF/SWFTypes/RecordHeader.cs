#region

using System;
using System.IO;

#endregion

namespace LegendaryClient.Logic.SWF.SWFTypes
{
    internal class RecordHeader
    {
        private bool _longTag;
        private ushort _tagCode;
        private uint _tagLength;

        public RecordHeader()
        {
        }

        public RecordHeader(int tag, int length)
        {
            _tagCode = Convert.ToUInt16(tag);
            _tagLength = Convert.ToUInt32(length);
            _longTag = _tagLength > 0x3e;
        }

        public RecordHeader(bool longTag)
        {
            _longTag = longTag;
        }

        public RecordHeader(int tag, int length, bool longTag)
        {
            _tagCode = Convert.ToUInt16(tag);
            _tagLength = Convert.ToUInt32(length);
            _longTag = longTag;
        }

        public RecordHeader(int tag, uint length)
        {
            _tagCode = Convert.ToUInt16(tag);
            _tagLength = length;
            _longTag = _tagLength > 0x3e;
        }

        public int TagCode
        {
            get { return _tagCode; }
            set { _tagCode = (ushort) value; }
        }

        public uint TagLength
        {
            get { return _tagLength; }
            set { _tagLength = value; }
        }

        public void ReadData(BinaryReader binaryReader)
        {
            ushort tagCl = binaryReader.ReadUInt16();
            _tagCode = Convert.ToUInt16(tagCl >> 6);
            _tagLength = Convert.ToUInt32(tagCl - (_tagCode << 6));


            if (_tagLength == 0x3F)
            {
                uint len = binaryReader.ReadUInt32();
                _tagLength = len;
                _longTag = (_tagLength <= 127);
            }
            else
            {
                _longTag = false;
            }

            if (_tagLength > binaryReader.BaseStream.Length)
            {
                throw new Exception("Invalid Tag Length");
            }
        }
    }
}