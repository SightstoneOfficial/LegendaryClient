using PVPNetConnect.RiotObjects.Platform.Game;
using System;

namespace PVPNetConnect.RiotObjects.Platform.Reroll.Pojo
{
    public class AramPlayerParticipant : Participant
    {
        public override string TypeName
        {
            get { return this.type; }
        }

        private string type = "com.riotgames.platform.reroll.pojo.AramPlayerParticipant";

        public AramPlayerParticipant()
        {
        }

        public AramPlayerParticipant(Callback callback)
        {
            this.callback = callback;
        }

        public AramPlayerParticipant(TypedObject result)
        {
            base.SetFields(this, result);
        }

        public new delegate void Callback(AramPlayerParticipant result);

        private Callback callback;

        public override void DoCallback(TypedObject result)
        {
            base.SetFields(this, result);
            callback(this);
        }

        [InternalName("timeAddedToQueue")]
        public Double TimeAddedToQueue { get; set; }

        [InternalName("index")]
        public Int32 Index { get; set; }

        [InternalName("queueRating")]
        public Int32 QueueRating { get; set; }

        [InternalName("accountId")]
        public Double AccountId { get; set; }

        [InternalName("botDifficulty")]
        public String BotDifficulty { get; set; }

        [InternalName("originalAccountNumber")]
        public Double OriginalAccountNumber { get; set; }

        [InternalName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [InternalName("minor")]
        public Boolean Minor { get; set; }

        [InternalName("locale")]
        public object Locale { get; set; }

        [InternalName("lastSelectedSkinIndex")]
        public Int32 LastSelectedSkinIndex { get; set; }

        [InternalName("partnerId")]
        public String PartnerId { get; set; }

        [InternalName("profileIconId")]
        public Int32 ProfileIconId { get; set; }

        [InternalName("teamOwner")]
        public Boolean TeamOwner { get; set; }

        [InternalName("pointSummary")]
        public PointSummary PointSummary { get; set; }

        [InternalName("summonerId")]
        public Double SummonerId { get; set; }

        [InternalName("badges")]
        public Int32 Badges { get; set; }

        [InternalName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [InternalName("clientInSynch")]
        public Boolean ClientInSynch { get; set; }

        [InternalName("summonerName")]
        public String SummonerName { get; set; }

        [InternalName("pickMode")]
        public Int32 PickMode { get; set; }

        [InternalName("originalPlatformId")]
        public String OriginalPlatformId { get; set; }

        [InternalName("teamParticipantId")]
        public Double TeamParticipantId { get; set; }
    }
}