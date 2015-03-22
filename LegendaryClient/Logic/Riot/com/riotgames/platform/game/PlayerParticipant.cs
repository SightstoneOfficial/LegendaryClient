using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.PlayerParticipant")]
    public class PlayerParticipant : Participant
    {
        [SerializedName("timeAddedToQueue")]
        public object TimeAddedToQueue { get; set; }

        [SerializedName("index")]
        public Int32 Index { get; set; }

        [SerializedName("queueRating")]
        public Int32 QueueRating { get; set; }

        [SerializedName("accountId")]
        public Double AccountId { get; set; }

        [SerializedName("botDifficulty")]
        public String BotDifficulty { get; set; }

        [SerializedName("originalAccountNumber")]
        public Double OriginalAccountNumber { get; set; }

        [SerializedName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [SerializedName("minor")]
        public Boolean Minor { get; set; }

        [SerializedName("locale")]
        public object Locale { get; set; }

        [SerializedName("lastSelectedSkinIndex")]
        public Int32 LastSelectedSkinIndex { get; set; }

        [SerializedName("partnerId")]
        public String PartnerId { get; set; }

        [SerializedName("profileIconId")]
        public Int32 ProfileIconId { get; set; }

        [SerializedName("teamOwner")]
        public Boolean TeamOwner { get; set; }

        [SerializedName("summonerId")]
        public Double SummonerId { get; set; }

        [SerializedName("badges")]
        public Int32 Badges { get; set; }

        [SerializedName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [SerializedName("clientInSynch")]
        public Boolean ClientInSynch { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("pickMode")]
        public Int32 PickMode { get; set; }

        [SerializedName("originalPlatformId")]
        public String OriginalPlatformId { get; set; }

        [SerializedName("teamParticipantId")]
        public object TeamParticipantId { get; set; }

        [SerializedName("pointSummary")]
        public PointSummary PointSummary { get; set; } 
    }
}
