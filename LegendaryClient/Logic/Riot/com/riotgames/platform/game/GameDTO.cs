using System;
using RtmpSharp.IO;
using System.Collections.Generic;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.GameDTO")]
    public class GameDTO
    {
        [SerializedName("spectatorsAllowed")]
        public String SpectatorsAllowed { get; set; }

        [SerializedName("passwordSet")]
        public Boolean PasswordSet { get; set; }

        [SerializedName("gameType")]
        public String GameType { get; set; }

        [SerializedName("gameTypeConfigId")]
        public Int32 GameTypeConfigId { get; set; }

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
        public Int32 GlmSecurePort { get; set; }

        [SerializedName("id")]
        public Double Id { get; set; }

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
        public Int32 SpectatorDelay { get; set; }

        [SerializedName("teamOne")]
        public List<Participant> TeamOne { get; set; }

        [SerializedName("terminatedCondition")]
        public String TerminatedCondition { get; set; }

        [SerializedName("queueTypeName")]
        public String QueueTypeName { get; set; }

        [SerializedName("glmPort")]
        public Int32 GlmPort { get; set; }

        [SerializedName("passbackUrl")]
        public object PassbackUrl { get; set; }

        [SerializedName("roomPassword")]
        public String RoomPassword { get; set; }

        [SerializedName("optimisticLock")]
        public Double OptimisticLock { get; set; }

        [SerializedName("maxNumPlayers")]
        public Int32 MaxNumPlayers { get; set; }

        [SerializedName("queuePosition")]
        public Int32 QueuePosition { get; set; }

        [SerializedName("gameMode")]
        public String GameMode { get; set; }

        [SerializedName("expiryTime")]
        public Double ExpiryTime { get; set; }

        [SerializedName("mapId")]
        public Int32 MapId { get; set; }

        [SerializedName("banOrder")]
        public List<Int32> BanOrder { get; set; }

        [SerializedName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [SerializedName("gameStateString")]
        public String GameStateString { get; set; }

        [SerializedName("playerChampionSelections")]
        public List<PlayerChampionSelectionDTO> PlayerChampionSelections { get; set; }

        [SerializedName("joinTimerDuration")]
        public Int32 JoinTimerDuration { get; set; }

        [SerializedName("passbackDataPacket")]
        public object PassbackDataPacket { get; set; }
    }
}
