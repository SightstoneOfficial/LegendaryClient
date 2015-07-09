using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Threading.Tasks;
using LegendaryClient.Logic.MultiUser;

namespace LegendaryClient.Scripting_Environment
{
	public class Plugin_Core
	{
		public Dictionary<string, object> variables;
        public Plugin_Core()
		{
			//Client.onChatMessageReceived += Client_onChatMessageReceived;
			//variables = new Dictionary<string, object> { { "LogToFile", new Action<string>(LogToFile) }, { "Username", Client.LoginPacket.AllSummonerData.Summoner.Name }, { "sendMessage", new Action<string, string>(Client.SendMessage) } };

		}

		private List<Script> loadedScripts = new List<Script>();
		private List<string> loadedScriptNamesIdentifier = new List<string>();
		private List<string> loadedScriptNamesVisual = new List<string>();


		Logger LogWindow = new Logger();


		public void log(string text)
		{ 
			if (!LogWindow.IsVisible)
			{
				LogWindow.Visibility = System.Windows.Visibility.Visible;
				LogWindow.Show();
			}
			
			LogWindow.Log(text);
			
        }

		private void Client_onChatMessageReceived(string sender, string Message)
		{
			CallFunctions("onMessage", sender, Message);
		}

		private void LogToFile(object var)
		{
			LCLog.WriteToLog.Log(var.ToString());
		}

		public void ClearConsole()
		{
			LogWindow.Clear();
		}

		public void LoadScript(string Path, string name)
		{
			if (loadedScriptNamesIdentifier.Contains(name))
				return;
			var skript = new Script(Path, name, this);
			foreach(var variable in variables.Keys)
			{
				skript.addVar(variable, variables[variable]);
			}
			skript.setup();
			loadedScriptNamesIdentifier.Add(name);
			loadedScriptNamesVisual.Add(skript.getName());
			loadedScripts.Add(skript);
		}

		public List<string> getAllLoadedPlugins()
		{
			return loadedScriptNamesVisual;
		}

		public void runAll()
		{
			foreach(var skript in loadedScripts)
			{
				skript.run();
			}
		}

		public void CallFunctions(string funcName)
		{
			foreach(var skript in loadedScripts)
			{
				skript.callFunc(funcName);
			}
		}
		public void CallFunctions(string funcName, object par1, object par2)
		{
			foreach (var skript in loadedScripts)
			{
				skript.callFunc(funcName, par1, par2);
			}
		}
	}
}
