using System.Collections.Generic;
using System.Linq;

namespace LCDevWindow.Commands.LegendaryClient
{
    public sealed class SendOverlay : LCCommand
    {
        public override List<string> HelpTips()
        {
            List<string> result = new List<string>
            {
                "Send an overlay to LegendaryClient",
                "Usage: SendOverlay(string (1), string (2), [string (3)]) -> string (1) = Title; string (2) = Content; string (FullOVER)"
            };
            return result;
        }
        public override string CommandName
        {
            get { return "SendOverlay(string, string, [string])"; }
        }
        public override object ActivateCommand(string[] args)
        {
            if (args.Count() <= 2) return null;
            if (args.Count() == 2)
                Main.SendPIPE.WriteString("SendOVERLAY|" + args[0] + "|" + args[1]);
            else
                Main.SendPIPE.WriteString("SendOVERLAY" + args[2] + "|" + args[0] + "|" + args[1]);
            return null;
        }
    }
}
