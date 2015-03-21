using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Media;

namespace LCDevWindow.Commands
{
    public sealed class CreateVar : Command
    {
        public override object ActivateCommand(string[] args)
        {
            string arg = args[0];
            try
            {
                string[] Json = Regex.Split(arg, "|");
                JavaScriptSerializer asdf = new JavaScriptSerializer();
                Dictionary<string, object> Result = asdf.Deserialize<Dictionary<string, object>>(Json[1]);
                Main.Vars.Add(Json[0], Result);
                return true;
            }
            catch (Exception except)
            {
                Main.win.Log("Error with the ArgPos value. Here are the error details: " + except.Message, Brushes.Red);
                return false;
            }
        }
        public override string CommandName
        {
            get { return "CreateVar"; }
        }
        public override List<string> HelpTips()
        {
            List<string> tips = new List<string>();
            tips.Add("Adds a var to add to the Main class, and can be used for calling objects with PVPNetConnect");
            tips.Add("Usage: CreateVar(JsonName + | + Json (As string)) -> Json to add to Main.cs");
            return tips;
        }
    }
}
