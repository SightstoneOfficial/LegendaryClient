#region

using System;
using LegendaryClient.Controls;

#endregion

namespace LegendaryClient.Logic
{
    public class InviteInfo
    {
        internal InvitationRequest stats { get; set; }
        internal GameInvitePopup popup { get; set; }
        internal bool PopupVisible { get; set; }
        internal string Inviter { get; set; }
    }
}