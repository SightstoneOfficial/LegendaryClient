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
        public string Username { get; set; }

        [InternalName("securityAnswer")]
        public object SecurityAnswer { get; set; }

        [InternalName("password")]
        public string Password { get; set; }

        [InternalName("partnerCredentials")]
        public object PartnerCredentials { get; set; }

        [InternalName("domain")]
        public string Domain { get; set; }

        [InternalName("ipAddress")]
        public string IpAddress { get; set; }

        [InternalName("clientVersion")]
        public string ClientVersion { get; set; }

        [InternalName("locale")]
        public string Locale { get; set; }

        [InternalName("authToken")]
        public string AuthToken { get; set; }

        [InternalName("operatingSystem")]
        public string OperatingSystem { get; set; }
    }
}