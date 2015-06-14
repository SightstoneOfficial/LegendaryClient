using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;

namespace LegendaryClient.Scripting_Environment
{
	public class Script
	{
		private Interpreter ScriptExec;
		private string name;
		public Script(string path, string name)
		{
			this.name = name;
			ScriptExec = new Interpreter();
			ScriptExec.loadCode(path);

		}

		private void log(object obj)
		{
			Plugin_Core.log(name + ": " + obj.ToString());
		}

		public void run()
		{
			ScriptExec.runCode();
		}

		public void addVar(string name, object value)
		{
			ScriptExec.addVar(name, value);
		}

		public void callFunc(string name)
		{
			var func = ScriptExec.getValue(name);
			if (!(func is Exception))
			{
				func = func as Action;
				func();
			}
				
		}
	}
}
