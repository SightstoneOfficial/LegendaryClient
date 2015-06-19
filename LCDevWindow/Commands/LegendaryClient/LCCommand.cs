using System;
using System.Collections.Generic;

namespace LCDevWindow.Commands.LegendaryClient
{
    // ReSharper disable once InconsistentNaming
    public abstract class LCCommand
    {
        public abstract object ActivateCommand(string[] args);

        public abstract List<string> HelpTips();

        public abstract string CommandName { get; }

        public static Command GetCommand(string command)
        {
            var t = Type.GetType("LCDevWindow.Commands." + command);

            if (t != null)
                return (Command)Activator.CreateInstance(t);
            t = Type.GetType("LCDevWindow.Commands.LegendaryClient." + command);

            if (t != null)
                return (Command)Activator.CreateInstance(t);

            return null;
        }
    }
}
