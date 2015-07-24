using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.GameDTO")]
    public class GameDTO
    {
        [SerializedName("spectatorsAllowed")]
        public String SpectatorsAllowed { get; set; }

        [SerializedName("passwordSet")]
        public bool PasswordSet { get; set; }

        [SerializedName("gameType")]
        public String GameType { get; set; }

        [SerializedName("gameTypeConfigId")]
        public int GameTypeConfigId { get; set; }

        [SerializedName("glmGameId")]
        public object GlmGameId { get; set; }

        [SerializedName("gameState")]
        public String GameState { get; set; }

        [SerializedName("glmHost")]
        public object GlmHost { get; set; }

        [SerializedName("observers")]
        public List<GameObserver> Observers { get; set; }

        [SerializedName("statusOfParticipants")]
        public object StatusOfParticipants { get; set; }

        [SerializedName("glmSecurePort")]
        public int GlmSecurePort { get; set; }

        [SerializedName("id")]
        public double Id { get; set; }

        [SerializedName("ownerSummary")]
        public PlayerParticipant OwnerSummary { get; set; }

        [SerializedName("teamTwo")]
        public List<Participant> TeamTwo { get; set; }

        [SerializedName("bannedChampions")]
        public List<BannedChampion> BannedChampions { get; set; }

        [SerializedName("roomName")]
        public String RoomName { get; set; }

        [SerializedName("name")]
        public String Name { get; set; }

        [SerializedName("spectatorDelay")]
        public int SpectatorDelay { get; set; }

        [SerializedName("teamOne")]
        public List<Participant> TeamOne { get; set; }

        [SerializedName("terminatedCondition")]
        public String TerminatedCondition { get; set; }

        [SerializedName("queueTypeName")]
        public String QueueTypeName { get; set; }

        [SerializedName("glmPort")]
        public int GlmPort { get; set; }

        [SerializedName("passbackUrl")]
        public object PassbackUrl { get; set; }

        [SerializedName("roomPassword")]
        public String RoomPassword { get; set; }

        [SerializedName("optimisticLock")]
        public double OptimisticLock { get; set; }

        [SerializedName("maxNumPlayers")]
        public int MaxNumPlayers { get; set; }

        [SerializedName("queuePosition")]
        public int QueuePosition { get; set; }

        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("expiryTime")]
        public double ExpiryTime { get; set; }

        [SerializedName("mapId")]
        public int MapId { get; set; }

        [SerializedName("banOrder")]
        public List<int> BanOrder { get; set; }

        [SerializedName("pickTurn")]
        public int PickTurn { get; set; }

        [SerializedName("gameStateString")]
        public String GameStateString { get; set; }

        [SerializedName("playerChampionSelections")]
        public List<PlayerChampionSelectionDTO> PlayerChampionSelections { get; set; }

        [SerializedName("joinTimerDuration")]
        public int JoinTimerDuration { get; set; }

        [SerializedName("passbackDataPacket")]
        public object PassbackDataPacket { get; set; }
    }
}
