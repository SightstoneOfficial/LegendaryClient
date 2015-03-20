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
        public string SpectatorsAllowed { get; set; }

        [InternalName("passwordSet")]
        public Boolean PasswordSet { get; set; }

        [InternalName("gameType")]
        public string GameType { get; set; }

        [InternalName("gameTypeConfigId")]
        public int GameTypeConfigId { get; set; }

        [InternalName("glmGameId")]
        public object GlmGameId { get; set; }

        [InternalName("gameState")]
        public string GameState { get; set; }

        [InternalName("glmHost")]
        public object GlmHost { get; set; }

        [InternalName("observers")]
        public List<GameObserver> Observers { get; set; }

        [InternalName("statusOfParticipants")]
        public object StatusOfParticipants { get; set; }

        [InternalName("glmSecurePort")]
        public int GlmSecurePort { get; set; }

        [InternalName("id")]
        public Double Id { get; set; }

        [InternalName("ownerSummary")]
        public PlayerParticipant OwnerSummary { get; set; }

        [InternalName("teamTwo")]
        public List<Participant> TeamTwo { get; set; }

        [InternalName("bannedChampions")]
        public List<BannedChampion> BannedChampions { get; set; }

        [InternalName("roomName")]
        public string RoomName { get; set; }

        [InternalName("name")]
        public string Name { get; set; }

        [InternalName("spectatorDelay")]
        public int SpectatorDelay { get; set; }

        [InternalName("teamOne")]
        public List<Participant> TeamOne { get; set; }

        [InternalName("terminatedCondition")]
        public string TerminatedCondition { get; set; }

        [InternalName("queueTypeName")]
        public string QueueTypeName { get; set; }

        /*[InternalName("featuredGameInfo")]
        public featuredGameInfo //*/

        [InternalName("glmPort")]
        public int GlmPort { get; set; }

        [InternalName("passbackUrl")]
        public object PassbackUrl { get; set; }

        [InternalName("roomPassword")]
        public string RoomPassword { get; set; }

        [InternalName("optimisticLock")]
        public Double OptimisticLock { get; set; }

        [InternalName("maxNumPlayers")]
        public int MaxNumPlayers { get; set; }

        [InternalName("queuePosition")]
        public int QueuePosition { get; set; }

        [InternalName("gameMode")]
        public string GameMode { get; set; }

        [InternalName("expiryTime")]
        public Double ExpiryTime { get; set; }

        [InternalName("mapId")]
        public int MapId { get; set; }

        [InternalName("banOrder")]
        public List<Int32> BanOrder { get; set; }

        [InternalName("pickTurn")]
        public int PickTurn { get; set; }

        [InternalName("gameStateString")]
        public string GameStateString { get; set; }

        [InternalName("playerChampionSelections")]
        public List<PlayerChampionSelectionDTO> PlayerChampionSelections { get; set; }

        [InternalName("joinTimerDuration")]
        public int JoinTimerDuration { get; set; }

        [InternalName("passbackDataPacket")]
        public object PassbackDataPacket { get; set; }

        [InternalName("gameMutators")]
        public List<string> GameMutators { get; set; }
    }
}