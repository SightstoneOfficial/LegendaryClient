using System;

namespace PVPNetConnect.RiotObjects.Platform.Login
{
    public class AuthenticationCredentials : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.login.AuthenticationCredentials";

        public AuthenticationCredentials()
        {
        }

        public AuthenticationCredentials(Callback callback)
        {
            this.callback = callback;
        }

        public AuthenticationCredentials(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(AuthenticationCredentials result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("oldPassword")]
        public object OldPassword { get; set; }

        [InternalName("username")]
        public String Username { get; set; }

        [InternalName("securityAnswer")]
        public object SecurityAnswer { get; set; }

        [InternalName("password")]
        public String Password { get; set; }

        [InternalName("partnerCredentials")]
        public object PartnerCredentials { get; set; }

        [InternalName("domain")]
        public String Domain { get; set; }

        [InternalName("ipAddress")]
        public String IpAddress { get; set; }

        [InternalName("clientVersion")]
        public String ClientVersion { get; set; }

        [InternalName("locale")]
        public String Locale { get; set; }

        [InternalName("authToken")]
        public String AuthToken { get; set; }

        [InternalName("operatingSystem")]
        public String OperatingSystem { get; set; }
    }
}