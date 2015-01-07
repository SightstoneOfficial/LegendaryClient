using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDevWindow.Commands.LegendaryClient
{
    public abstract class LCCommand
    {
        public abstract object ActivateCommand(string[] args);

        public abstract List<String> HelpTips();

        public abstract string CommandName { get; }

        public static Command GetCommand(String Command)
        {
            Type t = Type.GetType("LCDevWindow.Commands." + Command);

            if (t != null)
                return (Command)Activator.CreateInstance(t);
            t = Type.GetType("LCDevWindow.Commands.LegendaryClient." + Command);

            if (t != null)
                return (Command)Activator.CreateInstance(t);

            return null;
        }
    }
}
