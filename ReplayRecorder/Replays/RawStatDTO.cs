﻿using RtmpSharp.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LegendaryClient.Logic.Replays
{
    [Serializable]
    [SerializedName("com.riotgames.platform.observer.domain.RawStatDTO")]
    public class RawStatDTO
    {
        [SerializedName("value")]
        public Double Value { get; set; }

        [SerializedName("statTypeName")]
        public String StatTypeName { get; set; }
    }
}
