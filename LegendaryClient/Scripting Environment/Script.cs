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
			this.name = name.Replace(".py", "");
            ScriptExec = new Interpreter();
			ScriptExec.addVar("Log", new Action<object>(log));
			ScriptExec.loadCode(path);
			try
			{
				name = ScriptExec.getValue("PluginName");
			}
			catch(Exception e)
			{
				log("A valid name was not defined");
			}

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
