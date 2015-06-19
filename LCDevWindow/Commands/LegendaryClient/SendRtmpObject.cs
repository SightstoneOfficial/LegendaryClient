using System.Collections.Generic;

namespace LCDevWindow.Commands.LegendaryClient
{
    public sealed class SendRtmpObject : LCCommand
    {
        public override List<string> HelpTips()
        {
            List<string> result = new List<string>
            {
                "Send an RtmpObject (AsObject) to riot",
                "Usage: SendRtmpObject(string (1), string (2), string OR LocalVar) -> string (1) = Method Group; string (2) = Method Name; Extra Args to be send"
            };
            return result;
        }
        public override string CommandName
        {
            get { return "SendRtmpObject(string, string, [string])"; }
        }
        public override object ActivateCommand(string[] args)
        {
            Main.SendPIPE.WriteString("SendRtmpObject" + args[0] + "|" + args[1] + "|" + args[2]);
            return null;
        }
    }
}
