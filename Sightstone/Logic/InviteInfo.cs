#region

using System;
using Sightstone.Controls;
using Sightstone.Logic.Riot.Platform;

#endregion

namespace Sightstone.Logic
{
    public class InviteInfo
    {
        internal InvitationRequest stats { get; set; }
        internal GameInvitePopup popup { get; set; }
        internal bool PopupVisible { get; set; }
        internal string Inviter { get; set; }
    }
}