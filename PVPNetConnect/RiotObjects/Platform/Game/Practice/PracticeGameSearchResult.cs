using System;

namespace PVPNetConnect.RiotObjects.Platform.Game.Practice
{
    public class PracticeGameSearchResult : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.practice.PracticeGameSearchResult";

        public PracticeGameSearchResult()
        {
        }

        public PracticeGameSearchResult(Callback callback)
        {
            this.callback = callback;
        }

        public PracticeGameSearchResult(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(PracticeGameSearchResult result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("spectatorCount")]
        public Int32 SpectatorCount { get; set; }

        [InternalName("glmGameId")]
        public object GlmGameId { get; set; }

        [InternalName("glmHost")]
        public object GlmHost { get; set; }

        [InternalName("glmPort")]
        public Int32 GlmPort { get; set; }

        [InternalName("gameModeString")]
        public String GameModeString { get; set; }

        [InternalName("allowSpectators")]
        public String AllowSpectators { get; set; }

        [InternalName("gameMapId")]
        public Int32 GameMapId { get; set; }

        [InternalName("maxNumPlayers")]
        public Int32 MaxNumPlayers { get; set; }

        [InternalName("glmSecurePort")]
        public Int32 GlmSecurePort { get; set; }

        [InternalName("gameMode")]
        public String GameMode { get; set; }

        [InternalName("id")]
        public Double Id { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("privateGame")]
        public Boolean PrivateGame { get; set; }

        [InternalName("owner")]
        public PlayerParticipant Owner { get; set; }

        [InternalName("team1Count")]
        public Int32 Team1Count { get; set; }

        [InternalName("team2Count")]
        public Int32 Team2Count { get; set; }
    }
}