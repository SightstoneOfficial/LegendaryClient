using PVPNetConnect.RiotObjects.Platform.Reroll.Pojo;
using System;

namespace PVPNetConnect.RiotObjects.Platform.Game
{
    public class PlayerParticipant : GameParticipant
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.game.PlayerParticipant";

        public PlayerParticipant()
        {
        }

        public PlayerParticipant(Callback callback)
        {
            this.callback = callback;
        }

        public PlayerParticipant(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public new delegate void Callback(PlayerParticipant result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("timeAddedToQueue")]
        public new object TimeAddedToQueue { get; set; }

        [InternalName("index")]
        public int Index { get; set; }

        [InternalName("queueRating")]
        public int QueueRating { get; set; }

        [InternalName("accountId")]
        public double AccountId { get; set; }

        [InternalName("botDifficulty")]
        public string BotDifficulty { get; set; }

        [InternalName("originalAccountNumber")]
        public double OriginalAccountNumber { get; set; }

        [InternalName("summonerInternalName")]
        public new string SummonerInternalName { get; set; }

        [InternalName("minor")]
        public bool Minor { get; set; }

        [InternalName("locale")]
        public object Locale { get; set; }

        [InternalName("lastSelectedSkinIndex")]
        public int LastSelectedSkinIndex { get; set; }

        [InternalName("partnerId")]
        public string PartnerId { get; set; }

        [InternalName("profileIconId")]
        public int ProfileIconId { get; set; }

        [InternalName("teamOwner")]
        public bool TeamOwner { get; set; }

        [InternalName("summonerId")]
        public double SummonerId { get; set; }

        [InternalName("badges")]
        public int Badges { get; set; }

        [InternalName("pickTurn")]
        public int PickTurn { get; set; }

        [InternalName("clientInSynch")]
        public bool ClientInSynch { get; set; }

        [InternalName("summonerName")]
        public new string SummonerName { get; set; }

        [InternalName("pickMode")]
        public int PickMode { get; set; }

        [InternalName("pointSummary")]
        public PointSummary PointSummary { get; set; } 

        [InternalName("originalPlatformId")]
        public string OriginalPlatformId { get; set; }

        [InternalName("teamParticipantId")]
        public object TeamParticipantId { get; set; }
    }
}