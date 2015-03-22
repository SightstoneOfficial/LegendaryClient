using System;
using RtmpSharp.IO;

namespace LegendaryClient.Logic.Riot.Platform
{
    [Serializable]
    [SerializedName("com.riotgames.platform.gameinvite.contract.InvitePrivileges")]
    public class InvitePrivileges
    {
        [SerializedName("canInvite")]
        public bool canInvite { get; set; }
    }
}
