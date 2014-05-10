using System;

namespace PVPNetConnect.RiotObjects.Platform.Account
{
    public class AccountSummary : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.account.AccountSummary";

        public AccountSummary()
        {
        }

        public AccountSummary(Callback callback)
        {
            this.callback = callback;
        }

        public AccountSummary(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(AccountSummary result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("groupCount")]
        public Int32 GroupCount { get; set; }

        [InternalName("username")]
        public String Username { get; set; }

        [InternalName("accountId")]
        public Double AccountId { get; set; }

        [InternalName("summonerInternalName")]
        public object SummonerInternalName { get; set; }

        [InternalName("admin")]
        public Boolean Admin { get; set; }

        [InternalName("hasBetaAccess")]
        public Boolean HasBetaAccess { get; set; }

        [InternalName("summonerName")]
        public object SummonerName { get; set; }

        [InternalName("partnerMode")]
        public Boolean PartnerMode { get; set; }

        [InternalName("needsPasswordReset")]
        public Boolean NeedsPasswordReset { get; set; }
    }
}