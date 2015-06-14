using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Threading.Tasks;

namespace LegendaryClient.Scripting_Environment
{
	class Plugin_Core
	{
		
		private static List<Script> loadedScripts = new List<Script>();
		private static List<string> loadedScriptNames = new List<string>();
		public static Dictionary<string, object> variables = new Dictionary<string, object> { {"LogToText", new Action<string, string>(LCLog.WriteToLog.Log) }};
		static Logger LogWindow = new Logger();

		public static void log(string text)
		{ 
			if (!LogWindow.IsVisible)
			{
				LogWindow.Visibility = System.Windows.Visibility.Visible;
				LogWindow.Show();
			}
				
			LogWindow.Log(text);
        }

		public static void LoadScript(string Path, string name)
		{
			if (loadedScriptNames.Contains(name))
				return;
			var skript = new Script(Path, name);
			loadedScriptNames.Add(name);
			foreach(var variable in variables.Keys)
			{
				skript.addVar(variable, variables[variable]);
			}
			loadedScripts.Add(skript);
		}

		public static void runAll()
		{
			foreach(var skript in loadedScripts)
			{
				skript.run();
			}
		}

		public static void CallFunctions(string funcName)
		{
			foreach(var skript in loadedScripts)
			{
				skript.callFunc(funcName);
			}
		}
	}
}
