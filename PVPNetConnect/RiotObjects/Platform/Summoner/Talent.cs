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
        public Int32 Index { get; set; }

        [InternalName("level5Desc")]
        public String Level5Desc { get; set; }

        [InternalName("minLevel")]
        public Int32 MinLevel { get; set; }

        [InternalName("maxRank")]
        public Int32 MaxRank { get; set; }

        [InternalName("level4Desc")]
        public String Level4Desc { get; set; }

        [InternalName("tltId")]
        public Int32 TltId { get; set; }

        [InternalName("level3Desc")]
        public String Level3Desc { get; set; }

        [InternalName("talentGroupId")]
        public Int32 TalentGroupId { get; set; }

        [InternalName("gameCode")]
        public Int32 GameCode { get; set; }

        [InternalName("minTier")]
        public Int32 MinTier { get; set; }

        [InternalName("prereqTalentGameCode")]
        public object PrereqTalentGameCode { get; set; }

        [InternalName("level2Desc")]
        public String Level2Desc { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("talentRowId")]
        public Int32 TalentRowId { get; set; }

        [InternalName("level1Desc")]
        public String Level1Desc { get; set; }
    }
}