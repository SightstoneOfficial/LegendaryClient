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
            Main.win.Log("LC Dev Window. Using pipes to connect to LC", Brushes.LightSeaGreen);
            Main.win.Log("LCDevWindow Local Commands", Brushes.LightSeaGreen);
            List<Command> commands = GetInstances<Command>();
            foreach (Command command in commands)
            {
                Main.win.Log(command.CommandName, Brushes.Green);
                foreach (String help in command.HelpTips())
                {
                    Main.win.Log(help, Brushes.Black);
                }
                //Create a gap between commands
                Main.win.Log("", Brushes.Green);
            }
            Main.win.Log("", Brushes.Green);
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
