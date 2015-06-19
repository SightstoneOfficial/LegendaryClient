using System.Collections.Generic;
using System.Windows.Media;

namespace LCDevWindow.Commands
{
    public sealed class DoLog : Command
    {
        public override string CommandName
        {
            get { return "DoLog(string)"; }
        }
        public override List<string> HelpTips()
        {
            var tips = new List<string>
            {
                "Adds text to the local Log window",
                "Usage: DoLog(string) -> string is the text to be added"
            };
            return tips;
        }
        public override object ActivateCommand(string[] args)
        {
            Main.win.Log(args[0], Brushes.Black);
            return null;
        }
    }
}
