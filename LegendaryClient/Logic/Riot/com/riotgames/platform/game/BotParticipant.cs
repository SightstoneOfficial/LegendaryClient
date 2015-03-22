using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.game.BotParticipant")]
    public class BotParticipant : Participant
    {
        [SerializedName("botSkillLevel")]
        public Int32 BotSkillLevel { get; set; }

        [SerializedName("champion")]
        public ChampionDTO Champion { get; set; }

        [SerializedName("botSkillLevelName")]
        public String BotSkillLevelName { get; set; }

        [SerializedName("teamId")]
        public String TeamId { get; set; }

        [SerializedName("isGameOwner")]
        public Boolean IsGameOwner { get; set; }

        [SerializedName("pickMode")]
        public Int32 PickMode { get; set; }

        [SerializedName("team")]
        public Int32 Team { get; set; }

        [SerializedName("summonerInternalName")]
        public String SummonerInternalName { get; set; }

        [SerializedName("pickTurn")]
        public Int32 PickTurn { get; set; }

        [SerializedName("badges")]
        public Int32 Badges { get; set; }

        [SerializedName("isMe")]
        public Boolean IsMe { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("teamName")]
        public object TeamName { get; set; }
    }
}
