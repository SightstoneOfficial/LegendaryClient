using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows.Media;

namespace LCDevWindow.Commands
{
    public sealed class GetVar : Command
    {
        public override object ActivateCommand(string[] args)
        {
            JavaScriptSerializer asdf = new JavaScriptSerializer();
            if (Main.Vars.ContainsKey(args[0]))
                Main.win.Log("This var contains: " + asdf.Serialize(Main.Vars[args[0]]), Brushes.Green);
            else
                Main.win.Log("This var does not exist", Brushes.Red);
            return null;
        }
        public override string CommandName
        {
            get { return "GetVar"; }
        }
        public override List<string> HelpTips()
        {
            List<string> tips = new List<string>
            {
                "Sees if a var contains a certain value",
                "Usage: GetVar(string) -> The Var name"
            };
            return tips;
        }
    }
}
