using LegendaryClient.Controls;
using PVPNetConnect.RiotObjects.Gameinvite.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
