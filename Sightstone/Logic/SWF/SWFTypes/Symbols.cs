using Sightstone.Logic.SWF.SWFTypes;

namespace Sightstone.Logic.SWF.SWFTypes
{
    public class Symbols : Tag
    {
        public Symbols(byte[] data)
        {
            TagCode = (int)TagCodes.Symbols;
            Data = data;
            byteCode = new BytecodeHolder(this);
        }
    }
}