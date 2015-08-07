using System.IO;

namespace Sightstone.Logic.SWF.SWFTypes
{
    public class JPEG : Tag
    {
        public JPEG(byte[] data)
        {
            TagCode = (int) TagCodes.DefineBitJPEG2;
            Data = data;
            byteCode = new BytecodeHolder(this);
        }
    }
}