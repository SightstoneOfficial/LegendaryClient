#region

using System.IO;

#endregion

namespace LegendaryClient.Logic.SWF.SWFTypes
{
    public class Rect
    {
        public uint Nbits { get; private set; }

        public int Xmin { get; private set; }

        public int Xmax { get; private set; }

        public int Ymin { get; private set; }

        public int Ymax { get; private set; }

        public static Rect ReadRect(BinaryReader b)
        {
            uint NumberOfBits = SWFReader.ReadUnsignedBits(b, 5);
            var returnRect = new Rect
            {
                Nbits = NumberOfBits,
                Xmin = SWFReader.ReadSignedBits(b, NumberOfBits),
                Xmax = SWFReader.ReadSignedBits(b, NumberOfBits),
                Ymin = SWFReader.ReadSignedBits(b, NumberOfBits),
                Ymax = SWFReader.ReadSignedBits(b, NumberOfBits)
            };
            return returnRect;
        }
    }
}