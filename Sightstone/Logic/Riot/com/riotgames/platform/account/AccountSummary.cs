using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.account.AccountSummary")]
    public class AccountSummary
    {
        [SerializedName("groupCount")]
        public int GroupCount { get; set; }

        [SerializedName("username")]
        public String Username { get; set; }

        [SerializedName("accountId")]
        public double AccountId { get; set; }

        [SerializedName("summonerInternalName")]
        public object SummonerInternalName { get; set; }

        [SerializedName("admin")]
        public bool Admin { get; set; }

        [SerializedName("hasBetaAccess")]
        public bool HasBetaAccess { get; set; }

        [SerializedName("summonerName")]
        public object SummonerName { get; set; }

        [SerializedName("partnerMode")]
        public bool PartnerMode { get; set; }

        [SerializedName("needsPasswordReset")]
        public bool NeedsPasswordReset { get; set; }
    }
}
