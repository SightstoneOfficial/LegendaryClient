using System;
using System.Collections.Generic;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class GameDTO : RiotGamesObject
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.GameDTO";

        public GameDTO()
        {
        }

        public GameDTO(Callback callback)
        {
            this.callback = callback;
        }

        public GameDTO(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public delegate void Callback(GameDTO result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("spectatorsAllowed")]
        public String SpectatorsAllowed { get; set; }

        [InternalName("passwordSet")]
        public Boolean PasswordSet { get; set; }

        [InternalName("gameType")]
        public String GameType { get; set; }

        [InternalName("gameTypeConfigId")]
        public Int32 GameTypeConfigId { get; set; }

        [InternalName("glmGameId")]
        public object GlmGameId { get; set; }

        [InternalName("gameState")]
        public String GameState { get; set; }

        [InternalName("glmHost")]
        public object GlmHost { get; set; }

        [InternalName("observers")]
        public List<GameObserver> Observers { get; set; }

        [InternalName("statusOfParticipants")]
        public object StatusOfParticipants { get; set; }

        [InternalName("glmSecurePort")]
        public Int32 GlmSecurePort { get; set; }

        [InternalName("id")]
        public Double Id { get; set; }

        [InternalName("ownerSummary")]
        public PlayerParticipant OwnerSummary { get; set; }

        [InternalName("teamTwo")]
        public List<Participant> TeamTwo { get; set; }

        [InternalName("bannedChampions")]
        public List<BannedChampion> BannedChampions { get; set; }

        [InternalName("roomName")]
        public String RoomName { get; set; }

        [InternalName("name")]
        public String Name { get; set; }

        [InternalName("spectatorDelay")]
        public Int32 SpectatorDelay { get; set; }

        [InternalName("teamOne")]
        public List<Participant> TeamOne { get; set; }

        [InternalName("terminatedCondition")]
        public String TerminatedCondition { get; set; }

        [InternalName("queueTypeName")]
        public String QueueTypeName { get; set; }

        /*[InternalName("featuredGameInfo")]
        public featuredGameInfo //*/

        [InternalName("glmPort")]
        public Int32 GlmPort { get; set; }

        [InternalName("passbackUrl")]
        public object PassbackUrl { get; set; }

        [InternalName("roomPassword")]
        public String RoomPassword { get; set; }

        [InternalName("optimisticLock")]
        public Double OptimisticLock { get; set; }

        [InternalName("maxNumPlayers")]
        public Int32 MaxNumPlayers { get; set; }

        [InternalName("queuePosition")]
        public Int32 QueuePosition { get; set; }

        [InternalName("gameMode")]
        public String GameMode { get; set; }

        [InternalName("expiryTime")]
        public Double ExpiryTime { get; set; }

        [InternalName("mapId")]
        public Int32 MapId { get; set; }

        [InternalName("banOrder")]
        public List<Int32> BanOrder { get; set; }

        [InternalName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [InternalName("gameStateString")]
        public String GameStateString { get; set; }

        [InternalName("playerChampionSelections")]
        public List<PlayerChampionSelectionDTO> PlayerChampionSelections { get; set; }

        [InternalName("joinTimerDuration")]
        public Int32 JoinTimerDuration { get; set; }

        [InternalName("passbackDataPacket")]
        public object PassbackDataPacket { get; set; }

        [InternalName("gameMutators")]
        public List<string> GameMutators { get; set; }
    }
}