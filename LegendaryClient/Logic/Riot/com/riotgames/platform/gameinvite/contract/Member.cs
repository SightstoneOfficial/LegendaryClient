using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.com.riotgames.platform.gameinvite.contract
{
    [Serializable]
    [SerializedName("com.riotgames.platform.gameinvite.contract.Invitee")]
    public class Member
    {
        [SerializedName("hasDelegatedInvitePower")]
        public bool hasDelegatedInvitePower { get; set; }

        [SerializedName("summonerName")]
        public string SummonerName { get; set; }

        [SerializedName("summonerId")]
        public double SummonerId { get; set; }
    }
}
