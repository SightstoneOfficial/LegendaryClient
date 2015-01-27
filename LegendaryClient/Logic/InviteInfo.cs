#region

using System;
using LegendaryClient.Controls;
using PVPNetConnect.RiotObjects.Platform.Gameinvite.Contract;

#endregion

namespace LegendaryClient.Logic
{
    public class InviteInfo
    {
        internal InvitationRequest stats { get; set; }
        internal GameInvitePopup popup { get; set; }
        internal Boolean PopupVisible { get; set; }
        internal String Inviter { get; set; }
    }
}