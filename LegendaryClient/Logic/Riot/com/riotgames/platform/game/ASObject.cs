using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.ASObject")]
    public class ASObject
    {
        [SerializedName("LEAVER_BUSTER_ACCESS_TOKEN")]		
        public string Token { get; set; }

        [SerializedName("TypeName")]		
        public object Tname { get; set; }
    }
}
