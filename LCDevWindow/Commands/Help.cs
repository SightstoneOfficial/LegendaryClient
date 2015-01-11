using LCDevWindow.Commands.LegendaryClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LCDevWindow.Commands
{
    public sealed class Help : Command
    {
        public override List<string> HelpTips()
        {
            List<String> result = new List<string>();
            result.Add("Displays all commands for LCDevWindow.exe");
            result.Add("Usage: Help() -> No args");
            return result;
        }
        public override string CommandName
        {
            get { return "Help()"; }
        }
        public override object ActivateCommand(string[] args)
        {
            Main.win.Log("Help for LC Dev Window", Brushes.LightSkyBlue);
            Main.win.Log("LC Dev Window. Using pipes to connect to LC", Brushes.LightSeaGreen);
            Main.win.Log("All commands are similar to using a c# class, where args are placed in brackets", Brushes.Blue);
            Main.win.Log("Some changes are that you do not use \"\" to enclose strings and placing a \",\" will do", Brushes.Blue);
            Main.win.Log("An example is: MyCommand(arg1,arg2,arg3...) You want no whitespace between args, only in the args, Also you do not need \";\"", Brushes.Blue);
            Main.win.Log("Args CANNOT CONTAIN ANY OF THESE CHARS:", Brushes.DarkRed);
            Main.win.Log("\",\" [Used to tell start of new args]", Brushes.DarkRed);
            Main.win.Log("\"(\" [Used to tell start of args]", Brushes.DarkRed);
            Main.win.Log("\")\" [Used to tell end of args]", Brushes.DarkRed);
            Main.win.Log("", Brushes.White);
            Main.win.Log("LCDevWindow Local Commands", Brushes.LightSeaGreen);
            List<Command> commands = GetInstances<Command>();
            foreach (Command command in commands)
            {
                Main.win.Log(command.CommandName, Brushes.Green);
                foreach (String help in command.HelpTips())
                {
                    Main.win.Log(help, Brushes.Black);
                }
            }
            Main.win.Log("LegendaryClient Commands", Brushes.LightSeaGreen);
            List<LCCommand> lccommands = GetInstances<LCCommand>();
            foreach (LCCommand command in lccommands)
            {
                Main.win.Log(command.CommandName, Brushes.Green);
                foreach (String help in command.HelpTips())
                {
                    Main.win.Log(help, Brushes.Black);
                }
                //Create a gap between commands
                Main.win.Log("", Brushes.Green);
            }

            return null;
        }

        internal static List<T> GetInstances<T>()
        {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.BaseType == (typeof(T)) && t.GetConstructor(Type.EmptyTypes) != null
                    select (T)Activator.CreateInstance(t)).ToList();
        }

    }
}
