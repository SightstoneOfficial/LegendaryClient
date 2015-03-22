using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
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
        public Double AcctId { get; set; }

        [SerializedName("helpFlag")]
        public Boolean HelpFlag { get; set; }

        [SerializedName("sumId")]
        public Double SumId { get; set; }

        [SerializedName("profileIconId")]
        public Int32 ProfileIconId { get; set; }

        [SerializedName("displayEloQuestionaire")]
        public Boolean DisplayEloQuestionaire { get; set; }

        [SerializedName("lastGameDate")]
        public DateTime LastGameDate { get; set; }

        [SerializedName("advancedTutorialFlag")]
        public Boolean AdvancedTutorialFlag { get; set; }

        [SerializedName("revisionDate")]
        public DateTime RevisionDate { get; set; }

        [SerializedName("revisionId")]
        public Double RevisionId { get; set; }

        [SerializedName("seasonOneTier")]
        public String SeasonOneTier { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("nameChangeFlag")]
        public Boolean NameChangeFlag { get; set; }

        [SerializedName("tutorialFlag")]
        public Boolean TutorialFlag { get; set; }

        [SerializedName("socialNetworkUserIds")]
        public List<object> SocialNetworkUserIds { get; set; }
    }
}
