using System;
using RtmpSharp.IO;

namespace Sightstone.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.PlayerParticipant")]
    public class PlayerParticipant : Participant
    {
        [SerializedName("timeAddedToQueue")]
        public object TimeAddedToQueue { get; set; }

        [SerializedName("index")]
        public int Index { get; set; }

        [SerializedName("queueRating")]
        public int QueueRating { get; set; }

        [SerializedName("accountId")]
        public double AccountId { get; set; }

        [SerializedName("botDifficulty")]
        public String BotDifficulty { get; set; }

        [SerializedName("originalAccountNumber")]
        public double OriginalAccountNumber { get; set; }

        [SerializedName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [SerializedName("minor")]
        public bool Minor { get; set; }

        [SerializedName("locale")]
        public object Locale { get; set; }

        [SerializedName("lastSelectedSkinIndex")]
        public int LastSelectedSkinIndex { get; set; }

        [SerializedName("partnerId")]
        public String PartnerId { get; set; }

        [SerializedName("profileIconId")]
        public int ProfileIconId { get; set; }

        [SerializedName("teamOwner")]
        public bool TeamOwner { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }

        [SerializedName("badges")]
        public int Badges { get; set; }

        [SerializedName("pickTurn")]
        public int PickTurn { get; set; }

        [SerializedName("clientInSynch")]
        public bool ClientInSynch { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("pickMode")]
        public int PickMode { get; set; }

        [SerializedName("originalPlatformId")]
        public String OriginalPlatformId { get; set; }

        [SerializedName("teamParticipantId")]
        public object TeamParticipantId { get; set; }

        [SerializedName("pointSummary")]
        public PointSummary PointSummary { get; set; } 
    }
}
