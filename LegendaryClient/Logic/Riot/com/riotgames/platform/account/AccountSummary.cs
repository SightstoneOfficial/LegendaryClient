using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.account.AccountSummary")]
    public class AccountSummary
    {
        [SerializedName("groupCount")]
        public Int32 GroupCount { get; set; }

        [SerializedName("username")]
        public String Username { get; set; }

        [SerializedName("accountId")]
        public Double AccountId { get; set; }

        [SerializedName("summonerInternalName")]
        public object SummonerInternalName { get; set; }

        [SerializedName("admin")]
        public Boolean Admin { get; set; }

        [SerializedName("hasBetaAccess")]
        public Boolean HasBetaAccess { get; set; }

        [SerializedName("summonerName")]
        public object SummonerName { get; set; }

        [SerializedName("partnerMode")]
        public Boolean PartnerMode { get; set; }

        [SerializedName("needsPasswordReset")]
        public Boolean NeedsPasswordReset { get; set; }
    }
}
