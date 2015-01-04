using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LCDevWindow.Commands.LegendaryClient
{
    public sealed class SendOverlay : Command
    {
        public override object ActivateCommand(string[] args)
        {
            if (args.Count() > 2)
            {
                if (args.Count() == 2)
                    Main.SendPIPE.WriteString("SendOVERLAY|" + args[0] + "|" + args[1]);
                else
                    Main.SendPIPE.WriteString("SendOVERLAY" + args[2] + "|" + args[0] + "|" + args[1]);
            }
            return null;
        }
    }
}
