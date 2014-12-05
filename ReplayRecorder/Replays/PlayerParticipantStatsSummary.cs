#region

using System;
using System.Collections.Generic;
using RtmpSharp.IO;

#endregion

namespace LegendaryClient.Logic.Replays
{
    [Serializable]
    [SerializedName("com.riotgames.platform.observer.domain.PlayerParticipantStatsSummary")]
    public class PlayerParticipantStatsSummary
    {
        [SerializedName("skinName")]
        public String SkinName { get; set; }

        [SerializedName("gameId")]
        public Double GameId { get; set; }

        [SerializedName("profileIconId")]
        public Int32 ProfileIconId { get; set; }

        [SerializedName("elo")]
        public Int32 Elo { get; set; }

        [SerializedName("leaver")]
        public Boolean Leaver { get; set; }

        [SerializedName("leaves")]
        public Double Leaves { get; set; }

        [SerializedName("teamId")]
        public Double TeamId { get; set; }

        [SerializedName("eloChange")]
        public Int32 EloChange { get; set; }

        [SerializedName("statistics")]
        public List<RawStatDTO> Statistics { get; set; }

        [SerializedName("level")]
        public Double Level { get; set; }

        [SerializedName("botPlayer")]
        public Boolean BotPlayer { get; set; }

        [SerializedName("isMe")]
        public Boolean IsMe { get; set; }

        [SerializedName("inChat")]
        public Boolean InChat { get; set; }

        [SerializedName("userId")]
        public Double UserId { get; set; }

        [SerializedName("spell2Id")]
        public Double Spell2Id { get; set; }

        [SerializedName("losses")]
        public Double Losses { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("wins")]
        public Double Wins { get; set; }

        [SerializedName("spell1Id")]
        public Double Spell1Id { get; set; }

        [SerializedName("teamInfo")]
        public TeamInfo TeamInfo { get; set; }

        [SerializedName("reportEnabled")]
        public Boolean ReportEnabled { get; set; }

        [SerializedName("kudosEnabled")]
        public Boolean KudosEnabled { get; set; }
    }
}