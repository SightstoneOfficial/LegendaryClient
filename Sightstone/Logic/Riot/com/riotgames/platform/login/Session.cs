using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.login.Session")]
    public class Session
    {
        [SerializedName("token")]
        public String Token { get; set; }

        [SerializedName("password")]
        public String Password { get; set; }

        [SerializedName("accountSummary")]
        public AccountSummary AccountSummary { get; set; }
    }
}
