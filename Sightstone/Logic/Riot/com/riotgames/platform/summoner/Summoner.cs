using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.summoner.Summoner")]
    public class Summoner
    {
        [SerializedName("seasonTwoTier")]
        public String SeasonTwoTier { get; set; }

        [SerializedName("internalName")]
        public String InternalName { get; set; }

        [SerializedName("acctId")]
        public double AcctId { get; set; }

        [SerializedName("helpFlag")]
        public bool HelpFlag { get; set; }

        [SerializedName("sumId")]
        public double SumId { get; set; }

        [SerializedName("profileIconId")]
        public int ProfileIconId { get; set; }

        [SerializedName("displayEloQuestionaire")]
        public bool DisplayEloQuestionaire { get; set; }

        [SerializedName("lastGameDate")]
        public DateTime LastGameDate { get; set; }

        [SerializedName("advancedTutorialFlag")]
        public bool AdvancedTutorialFlag { get; set; }

        [SerializedName("revisionDate")]
        public DateTime RevisionDate { get; set; }

        [SerializedName("revisionId")]
        public double RevisionId { get; set; }

        [SerializedName("seasonOneTier")]
        public String SeasonOneTier { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("nameChangeFlag")]
        public bool NameChangeFlag { get; set; }

        [SerializedName("tutorialFlag")]
        public bool TutorialFlag { get; set; }

        [SerializedName("socialNetworkUserIds")]
        public List<object> SocialNetworkUserIds { get; set; }
    }
}
