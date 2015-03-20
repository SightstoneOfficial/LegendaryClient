using System;

namespace PVPNetConnect.RiotObjects.Platform.Summoner
{
    public class Talent : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.summoner.Talent";

        public Talent()
        {
        }

        public Talent(Callback callback)
        {
            this.callback = callback;
        }

        public Talent(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(Talent result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("index")]
        public int Index { get; set; }

        [InternalName("level5Desc")]
        public string Level5Desc { get; set; }

        [InternalName("minLevel")]
        public int MinLevel { get; set; }

        [InternalName("maxRank")]
        public int MaxRank { get; set; }

        [InternalName("level4Desc")]
        public string Level4Desc { get; set; }

        [InternalName("tltId")]
        public int TltId { get; set; }

        [InternalName("level3Desc")]
        public string Level3Desc { get; set; }

        [InternalName("talentGroupId")]
        public int TalentGroupId { get; set; }

        [InternalName("gameCode")]
        public int GameCode { get; set; }

        [InternalName("minTier")]
        public int MinTier { get; set; }

        [InternalName("prereqTalentGameCode")]
        public object PrereqTalentGameCode { get; set; }

        [InternalName("level2Desc")]
        public string Level2Desc { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("talentRowId")]
        public int TalentRowId { get; set; }

        [InternalName("level1Desc")]
        public string Level1Desc { get; set; }
    }
}