using System.Collections.Generic;

namespace LCDevWindow.Commands.Sightstone
{
    public abstract class MessageReceived : LCCommand
    {
        public override List<string> HelpTips()
        {
            var result = new List<string>
            {
                "Used to tell the window what to do when a message is received",
                "Usage: MessageReceived(string (1), string (2)) -> string (1) = Receive Type; string (2) = Return Type;"
            };
            return result;
        }
        public override string CommandName
        {
            get { return "MessageReceived(string, string)"; }
        }
        public override object ActivateCommand(string[] args)
        {
            Main.SendPIPE.WriteString("MessageReceived" + args[0] + "|" + args[1]);
            return null;
        }
    }
}
