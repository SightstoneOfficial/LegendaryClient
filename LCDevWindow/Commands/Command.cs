using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LCDevWindow.Commands
{
    public abstract class Command
    {
        public abstract object ActivateCommand(string[] args);

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
