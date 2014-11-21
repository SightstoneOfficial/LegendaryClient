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

        [InternalName("botSkillLevel")]
        public Int32 BotSkillLevel { get; set; }

        [InternalName("champion")]
        public ChampionDTO Champion { get; set; }

        [InternalName("botSkillLevelName")]
        public String BotSkillLevelName { get; set; }

        [InternalName("teamId")]
        public String TeamId { get; set; }

        [InternalName("summonerName")]
        public new String SummonerName { get; set; }

    }
}