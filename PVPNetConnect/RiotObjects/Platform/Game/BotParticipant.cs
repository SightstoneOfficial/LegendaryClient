using PVPNetConnect.RiotObjects.Platform.Catalog.Champion;
using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class BotParticipant : Participant
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

        public delegate void Callback(BotParticipant result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("botSkillLevel")]
        public Int32 BotSkillLevel { get; set; }

        [InternalName("champion")]
        public ChampionDTO Champion { get; set; }

        [InternalName("botSkillLevelName")]
        public String BotSkillLevelName { get; set; }

        [InternalName("teamId")]
        public String TeamId { get; set; }

        [InternalName("isGameOwner")]
        public Boolean IsGameOwner { get; set; }

        [InternalName("pickMode")]
        public Int32 PickMode { get; set; }

        [InternalName("team")]
        public Int32 Team { get; set; }

        [InternalName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [InternalName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [InternalName("badges")]
        public Int32 Badges { get; set; }

        [InternalName("isMe")]
        public Boolean IsMe { get; set; }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }

        [InternalName("teamName")]
        public object TeamName { get; set; }
    }
}