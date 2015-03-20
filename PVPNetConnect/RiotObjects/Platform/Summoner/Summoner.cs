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
        public string InternalName { get; set; }

        [InternalName("previousSeasonHighestTier")]
        public string previousSeasonHighestTier { get; set; }

        [InternalName("acctId")]
        public Double AcctId { get; set; }

        [InternalName("helpFlag")]
        public Boolean HelpFlag { get; set; }

        [InternalName("sumId")]
        public Double SumId { get; set; }

        [InternalName("profileIconId")]
        public int ProfileIconId { get; set; }

        [InternalName("displayEloQuestionaire")]
        public Boolean DisplayEloQuestionaire { get; set; }

        [InternalName("lastGameDate")]
        public DateTime LastGameDate { get; set; }
        
        [InternalName("previousSeasonHighestTeamReward")]
        public int previousSeasonHighestTeamReward { get; set; }

        [InternalName("revisionDate")]
        public DateTime RevisionDate { get; set; }

        [InternalName("advancedTutorialFlag")]
        public Boolean AdvancedTutorialFlag { get; set; }

        [InternalName("revisionId")]
        public Double RevisionId { get; set; }

        //TODO: find out object type, it seems to be null for now
        [InternalName("futureData")]
        public object futureData { get; set; }

        [InternalName("dataVersion")]
        public int dataVersion { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("nameChangeFlag")]
        public Boolean NameChangeFlag { get; set; }

        [InternalName("tutorialFlag")]
        public Boolean TutorialFlag { get; set; }
    }
}