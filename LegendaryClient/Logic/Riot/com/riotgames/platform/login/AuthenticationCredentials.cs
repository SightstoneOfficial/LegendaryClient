using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.login.AuthenticationCredentials")]
    public class AuthenticationCredentials
    {
        [SerializedName("oldPassword")]
        public object OldPassword { get; set; }

        [SerializedName("username")]
        public String Username { get; set; }

        [SerializedName("secUrityAnswer")]
        public object SecUrityAnswer { get; set; }

        [SerializedName("password")]
        public String Password { get; set; }

        [SerializedName("partnerCredentials")]
        public object PartnerCredentials { get; set; }

        [SerializedName("domain")]
        public String Domain { get; set; }

        [SerializedName("ipAddress")]
        public String IpAddress { get; set; }

        [SerializedName("clientVersion")]
        public String ClientVersion { get; set; }

        [SerializedName("locale")]
        public String Locale { get; set; }

        [SerializedName("authToken")]
        public String AuthToken { get; set; }

        [SerializedName("operatingSystem")]
        public String OperatingSystem { get; set; }
    }
}
