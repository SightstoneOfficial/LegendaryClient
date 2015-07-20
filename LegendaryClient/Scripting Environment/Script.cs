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
		Plugin_Core Parent;
		public Script(string path, string name, Plugin_Core refer)
		{
			Parent = refer;
			this.name = name.Replace(".py", "");
            ScriptExec = new Interpreter();
			ScriptExec.addVar("Log", new Action<object>(log));
			ScriptExec.loadCode(path);
			

		}

		public string getName()
		{
			return name;
		}

		public void setup()
		{
			ScriptExec.runCode();
			Parent.ClearConsole();
			try
			{
				name = ScriptExec.getValue("PluginName");
			}
			catch (Exception e)
			{
				log("A valid name was not defined");

			}
		}

		private void log(object obj)
		{
			Parent.log(name + ": " + obj);
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

		internal void callFunc(string funcName, object par1, object par2)
		{
			var func = ScriptExec.getValue(name);
			if (!(func is Exception))
			{
				func = func as Action;
				func(par1, par2);
			}
		}

        internal void callFunc(string funcName, params object[] pars)
        {
            var func = ScriptExec.getValue(name);
            if (!(func is Exception))
            {
                func = func as Action;
                func(pars);
            }
        }
    }
}
