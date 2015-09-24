using System.IO;

namespace Sightstone.Logic.SWF.SWFTypes
{
    public class Lossless : Tag
    {
        public Lossless(byte[] data)
        {
            TagCode = (int) TagCodes.DefineBitLossless;
            Data = data;
            byteCode = new BytecodeHolder(this);
        }
    }
}