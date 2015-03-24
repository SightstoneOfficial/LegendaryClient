using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.gameinvite.contract.Invitee")]
    public class Invitee
    {
        [SerializedName("inviteeStateAsString")]
        public String InviteeState { get; set; }

        [SerializedName("summonerName")]
        public String SummonerName { get; set; }

        [SerializedName("inviteeState")]
        public string inviteeState { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }
    }
}
