using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class Summoner : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.Summoner";

        public Summoner()
        {
        }

        public Summoner(Callback callback)
        {
            this.callback = callback;
        }

        public Summoner(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Summoner result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("internalName")]
        public String InternalName { get; set; }

        [InternalName("previousSeasonHighestTier")]
        public String previousSeasonHighestTier { get; set; }

        [InternalName("acctId")]
        public Double AcctId { get; set; }

        [InternalName("helpFlag")]
        public Boolean HelpFlag { get; set; }

        [InternalName("sumId")]
        public Double SumId { get; set; }

        [InternalName("profileIconId")]
        public Int32 ProfileIconId { get; set; }

        [InternalName("displayEloQuestionaire")]
        public Boolean DisplayEloQuestionaire { get; set; }

        [InternalName("lastGameDate")]
        public DateTime LastGameDate { get; set; }
        
        [InternalName("previousSeasonHighestTeamReward")]
        public Int32 previousSeasonHighestTeamReward { get; set; }

        [InternalName("revisionDate")]
        public DateTime RevisionDate { get; set; }

        [InternalName("advancedTutorialFlag")]
        public Boolean AdvancedTutorialFlag { get; set; }

        [InternalName("revisionId")]
        public Double RevisionId { get; set; }

        //TODO: find out object type, it seems to be null for now
        [InternalName("futureData")]
        public Object futureData { get; set; }

        [InternalName("dataVersion")]
        public Int32 dataVersion { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("nameChangeFlag")]
        public Boolean NameChangeFlag { get; set; }

        [InternalName("tutorialFlag")]
        public Boolean TutorialFlag { get; set; }
    }
}