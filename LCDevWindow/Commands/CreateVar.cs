using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Windows.Media;

namespace LCDevWindow.Commands
{
    public sealed class CreateVar : Command
    {
        public override object ActivateCommand(string[] args)
        {
            try
            {
                var asdf = new JavaScriptSerializer();
                var result = asdf.Deserialize<Dictionary<string, object>>(args[1]);
                Main.Vars.Add(args[0], result);
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
            List<string> tips = new List<string>
            {
                "Adds a var to add to the Main class, and can be used for calling objects with PVPNetConnect",
                "Usage: CreateVar(JsonName,Json (As string)) -> Json to add to Main.cs"
            };
            return tips;
        }
    }
}
