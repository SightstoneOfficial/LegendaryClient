using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            List<String> tips = new List<String>();
            tips.Add("Adds text to the local Log window");
            tips.Add("Usage: DoLog(string) -> String is the text to be added");
            return tips;
        }
        public override object ActivateCommand(string[] args)
        {
            Main.win.Log((string)args[0], Brushes.Black);
            return null;
        }
    }
}
