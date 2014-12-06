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
            uint numberOfBits = SWFReader.ReadUnsignedBits(b, 5);
            var returnRect = new Rect
            {
                Nbits = numberOfBits,
                Xmin = SWFReader.ReadSignedBits(b, numberOfBits),
                Xmax = SWFReader.ReadSignedBits(b, numberOfBits),
                Ymin = SWFReader.ReadSignedBits(b, numberOfBits),
                Ymax = SWFReader.ReadSignedBits(b, numberOfBits)
            };
            return returnRect;
        }
    }
}