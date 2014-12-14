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
        public ChampionDTO Champion { get; set; }

        [InternalName("botSkillLevel")]
        public Int32 BotSkillLevel { get; set; }

        [InternalName("teamId")]
        public string teamId { get; set; }

        [InternalName("botSkillLevelName")]
        public string botSkillLevelName { get; set; }

        [InternalName("pickMode")]
        public Int32 pickMode { get; set; }

        [InternalName("isMe")]
        public Boolean IsMe { get; set; }

        [InternalName("summonerName")]
        public string SummonerName { get; set; }

        [InternalName("team")]
        public Int32 Team { get; set; }

        [InternalName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [InternalName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [InternalName("badges")]
        public Int32 Badges { get; set; }

        [InternalName("teamName")]
        public Object TeamName { get; set; }

        [InternalName("isGameOwner")]
        public Boolean IsGameOwner { get; set; }
    }
}