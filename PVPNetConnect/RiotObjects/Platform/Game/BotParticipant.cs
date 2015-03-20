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
        public int BotSkillLevel { get; set; }

        [InternalName("teamId")]
        public string teamId { get; set; }

        [InternalName("botSkillLevelName")]
        public string botSkillLevelName { get; set; }

        [InternalName("pickMode")]
        public int pickMode { get; set; }

        [InternalName("isMe")]
        public bool IsMe { get; set; }

        [InternalName("team")]
        public int Team { get; set; }

        [InternalName("pickTurn")]
        public int PickTurn { get; set; }

        [InternalName("badges")]
        public int Badges { get; set; }

        [InternalName("teamName")]
        public object TeamName { get; set; }

        [InternalName("isGameOwner")]
        public bool IsGameOwner { get; set; }

        [InternalName("summonerInternalName")]
        public new string SummonerInternalName { get; set; }
    }
}
