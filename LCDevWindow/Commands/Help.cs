using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LCDevWindow.Commands
{
    public sealed class Help : Command
    {
        public override object ActivateCommand(string[] args)
        {
            Main.win.Log("LC Dev Window. Using pipes to connect to LC", Brushes.Black);
            Main.win.Log("You can not have \",\" in your args, because it thinks that it is now a new arg", Brushes.Black);
            Main.win.Log("DoLog(string) -> Writes somthing to the log textbox", Brushes.Black);
            Main.win.Log("I will write more commands later, one rtmp command to send and read a message :P", Brushes.Black);
            return null;
        }
    }
}
