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
		public Script(string path)
		{
			ScriptExec = new Interpreter();
			ScriptExec.loadCode(path);

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
