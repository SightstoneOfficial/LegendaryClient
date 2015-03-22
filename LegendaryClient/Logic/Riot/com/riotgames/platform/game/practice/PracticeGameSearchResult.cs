using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.practice.PracticeGameSearchResult")]
    public class PracticeGameSearchResult
    {
        [SerializedName("spectatorCount")]
        public Int32 SpectatorCount { get; set; }

        [SerializedName("glmGameId")]
        public object GlmGameId { get; set; }

        [SerializedName("glmHost")]
        public object GlmHost { get; set; }

        [SerializedName("glmPort")]
        public Int32 GlmPort { get; set; }

        [SerializedName("gameModeString")]
        public String GameModeString { get; set; }

        [SerializedName("allowSpectators")]
        public String AllowSpectators { get; set; }

        [SerializedName("gameMapId")]
        public Int32 GameMapId { get; set; }

        [SerializedName("maxNumPlayers")]
        public Int32 MaxNumPlayers { get; set; }

        [SerializedName("glmSecurePort")]
        public Int32 GlmSecurePort { get; set; }

        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("id")]
        public Double Id { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("privateGame")]
        public Boolean PrivateGame { get; set; }

        [SerializedName("owner")]
        public PlayerParticipant Owner { get; set; }

        [SerializedName("team1Count")]
        public Int32 Team1Count { get; set; }

        [SerializedName("team2Count")]
        public Int32 Team2Count { get; set; }
    }
}
