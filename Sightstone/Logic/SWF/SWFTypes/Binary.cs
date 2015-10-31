using System.IO;

namespace Sightstone.Logic.SWF.SWFTypes
{
    public class Binary : Tag
    {
        public Binary(byte[] data)
        {
            TagCode = (int) TagCodes.Binary;
            Data = data;
            byteCode = new BytecodeHolder(this);
        }
    }
}