using LCDevWindow.Commands.Sightstone;
using System;
using System.Collections.Generic;

namespace LCDevWindow.Commands
{
    public abstract class Command
    {
        public abstract object ActivateCommand(string[] args);

        public abstract List<string> HelpTips();

        public abstract string CommandName { get; }

        public static object GetCommand(string command)
        {
            var t = Type.GetType("LCDevWindow.Commands." + command);

            if (t != null)
                return (Command)Activator.CreateInstance(t);
            t = Type.GetType("LCDevWindow.Commands.Sightstone." + command);

            if (t != null)
                return (LCCommand)Activator.CreateInstance(t);

            return null;
        }
    }
}
