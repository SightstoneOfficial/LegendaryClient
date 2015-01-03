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
        public override object ActivateCommand(string[] args)
        {
            Main.win.Log((string)args[0], Brushes.Black);
            return null;
        }
    }
}
