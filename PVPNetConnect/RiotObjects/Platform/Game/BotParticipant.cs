using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class BotParticipant : GameParticipant
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.BotParticipant";

        public BotParticipant()
        {
        }

        public BotParticipant(Callback callback)
        {
            this.callback = callback;
        }

        public BotParticipant(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public new delegate void Callback(BotParticipant result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("champion")]
        public ChampionDTO champion { get; set; }

        [InternalName("botSkillLevel")]
        public int botSkillLevel { get; set; }

        [InternalName("teamId")]
        public string teamId { get; set; }

        [InternalName("botSkillLevelName")]
        public string botSkillLevelName { get; set; }

        [InternalName("pickMode")]
        public int pickMode { get; set; }

        [InternalName("isGameOwner")]
        public bool isGameOwner { get; set; }

        [InternalName("summonerInternalName")]
        public string SummonerInternalName { get; set; }

        [InternalName("pickTurn")]
        public int pickTurn { get; set; }

        [InternalName("isMe")]
        public bool isMe { get; set; }

        [InternalName("badges")]
        public int badges { get; set; }

        [InternalName("teamName")]
        public string teamName { get; set; }

        [InternalName("team")]
        public int team { get; set; }

        [InternalName("summonerName")]
        public string SummonerName { get; set; }

        //?
        [InternalName("dataVersion")]
        public object dataVersion = null;
        [InternalName("futureData")]
        public object futureData = null;
    }
}