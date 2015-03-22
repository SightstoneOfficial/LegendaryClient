using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.com.riotgames.platform.gameinvite.contract
{
    [Serializable]
    [SerializedName("com.riotgames.platform.gameinvite.contract.InvitePrivileges")]
    public class InvitePrivileges
    {
        [SerializedName("canInvite")]
        public bool canInvite { get; set; }
    }
}
