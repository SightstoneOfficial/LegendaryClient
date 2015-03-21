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
        public int SpectatorCount { get; set; }

        [InternalName("glmGameId")]
        public object GlmGameId { get; set; }

        [InternalName("glmHost")]
        public object GlmHost { get; set; }

        [InternalName("glmPort")]
        public int GlmPort { get; set; }

        [InternalName("gameModeString")]
        public string GameModeString { get; set; }

        [InternalName("allowSpectators")]
        public string AllowSpectators { get; set; }

        [InternalName("gameMapId")]
        public int GameMapId { get; set; }

        [InternalName("maxNumPlayers")]
        public int MaxNumPlayers { get; set; }

        [InternalName("glmSecurePort")]
        public int GlmSecurePort { get; set; }

        [InternalName("gameMode")]
        public string GameMode { get; set; }

        [InternalName("id")]
        public double Id { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("privateGame")]
        public bool PrivateGame { get; set; }

        [InternalName("owner")]
        public PlayerParticipant Owner { get; set; }

        [InternalName("team1Count")]
        public int Team1Count { get; set; }

        [InternalName("team2Count")]
        public int Team2Count { get; set; }
    }
}