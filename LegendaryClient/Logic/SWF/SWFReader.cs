#region

using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression;
using LegendaryClient.Logic.SWF.SWFTypes;

#endregion

namespace LegendaryClient.Logic.SWF
{
    public class SWFReader
    {
        private BinaryReader SWFBinary;

        public SWFReader(string SWFFile)
        {
            Tags = new List<Tag>();
            using (var b = new BinaryReader(File.Open(SWFFile, FileMode.Open)))
                if (b.PeekChar() == 'C') //Zlib Compressed
                    Uncompress(b);

            if (SWFBinary == null)
                SWFBinary = new BinaryReader(File.Open(SWFFile, FileMode.Open));

            ReadSWFHeader();

            bool readEndTag = false;
            while (SWFBinary.BaseStream.Position < SWFBinary.BaseStream.Length && !readEndTag)
            {
                Tag b = ReadTag();
                if (b == null)
                    continue;

                if (b is End)
                    readEndTag = true;

                Tags.Add(b);
            }
        }

        public SWFCompression SWFCompressionType { get; private set; }

        public byte SWFVersion { get; private set; }

        public UInt32 FileSize { get; private set; }

        public UInt16 FrameRate { get; private set; }

        public UInt16 FrameCount { get; private set; }

        public List<Tag> Tags { get; set; }

        internal Tag ReadTag()
        {
            long posBefore = SWFBinary.BaseStream.Position;
            var rh = new RecordHeader();
            rh.ReadData(SWFBinary);

            var offset = (int) (SWFBinary.BaseStream.Position - posBefore);
            SWFBinary.BaseStream.Position = posBefore;

            Tag resTag;

            switch (rh.TagCode)
            {
                case (int) TagCodes.DoABC:
                    resTag = new DoABC();
                    break;
                case (int) TagCodes.End:
                    resTag = new End();
                    break;
                default:
                    resTag = new Tag(SWFBinary.ReadBytes(Convert.ToInt32(rh.TagLength + offset)));
                    break;
            }

            resTag.ReadData(SWFVersion, SWFBinary);

            return resTag;
        }

        private void Uncompress(BinaryReader SWFBinary)
        {
            SWFBinary.BaseStream.Position = 4;
            int size = Convert.ToInt32(SWFBinary.ReadUInt32());

            var UncompressedData = new byte[size];
            SWFBinary.BaseStream.Position = 0;
            SWFBinary.Read(UncompressedData, 0, 8);

            byte[] CompressedData = SWFBinary.ReadBytes(size);
            var zipInflator = new Inflater();
            zipInflator.SetInput(CompressedData);
            zipInflator.Inflate(UncompressedData, 8, size - 8);

            var m = new MemoryStream(UncompressedData);
            this.SWFBinary = new BinaryReader(m);
        }

        private void ReadSWFHeader()
        {
            char CompressionType = SWFBinary.ReadChar();
            switch (CompressionType)
            {
                case 'C':
                    SWFCompressionType = SWFCompression.Zlib;
                    break;

                case 'Z':
                    SWFCompressionType = SWFCompression.LZMA;
                    throw new Exception("Cannot decompress this SWF type!");
                case 'F':
                    SWFCompressionType = SWFCompression.Uncompressed;
                    break;

                default:
                    throw new Exception("Unknown SWFCompression type");
            }

            SWFBinary.ReadChar(); //Always 'W'
            SWFBinary.ReadChar(); //Always 'S'

            SWFVersion = SWFBinary.ReadByte();
            FileSize = SWFBinary.ReadUInt32();

            if (FileSize != SWFBinary.BaseStream.Length)
                throw new Exception("Corrupt ClientLibCommon.dat");

            Rect FrameSize = Rect.ReadRect(SWFBinary);
            FrameRate = SWFBinary.ReadUInt16();
            FrameCount = SWFBinary.ReadUInt16();
        }

        #region Extention Buffer Methods

        internal static uint ReadUnsignedBits(BinaryReader b, uint bits)
        {
            uint v = 0;
            int bitPos = 0;
            uint bitBuf = 0;

            while (true)
            {
                var s = (int) (bits - bitPos);

                if (s > 0)
                {
                    v |= bitBuf << s;
                    bits -= (uint) bitPos;

                    bitBuf = b.ReadByte();
                    bitPos = 8;
                }
                else
                {
                    v |= bitBuf >> -s;

                    bitPos -= (int) bits;
                    bitBuf &= (uint) (0xff >> (8 - bitPos));

                    return v;
                }
            }
        }

        internal static int ReadSignedBits(BinaryReader b, uint bits)
        {
            var v = (int) (ReadUnsignedBits(b, bits));

            if ((v & (1L << (int) (bits - 1))) > 0)
            {
                v |= -1 << (int) bits;
            }

            return v;
        }

        #endregion Extention Buffer Methods
    }
}