﻿using System.IO;

namespace Sightstone.Logic.SWF.SWFTypes
{
    public class End : Tag
    {
        public End()
        {
            TagCode = (int) TagCodes.End;
        }

        public override void ReadData(byte version, BinaryReader binaryReader)
        {
            var rh = new RecordHeader();
            rh.ReadData(binaryReader);
        }
    }
}