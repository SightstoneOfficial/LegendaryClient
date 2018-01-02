﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Threading.Tasks;
using Sightstone.Logic.MultiUser;
using Sightstone.Windows;

namespace Sightstone.Scripting_Environment
{
	public class Plugin_Core
	{
		public Dictionary<string, object> variables = new Dictionary<string, object>();
        public Plugin_Core()
		{
            foreach (var cl in UserList.Users.SelectMany(user => user.Value.Where(userclients => userclients.Value.IsLoggedIn))) 
            {
                var cl1 = cl;
                cl.Value.onChatMessageReceived += (ob, se) => Client_onChatMessageReceived(ob, se, cl1.Key);
                variables.Add(cl.Key, new Dictionary <string, object> { { "LogToFile", new Action<string>(LogToFile) }, { "Username", cl.Key }, { "sendMessage", new Action<string, string>(cl.Value.SendMessage) } });
            }
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

		private void Client_onChatMessageReceived(string sender, string Message, string user)
		{
			CallFunctions("onMessage", sender, Message, user);
		}

		private void LogToFile(object var)
		{
			LCLog.WriteToLog.Log(var.ToString());
		}

		public void ClearConsole()
		{
			LogWindow.Clear();
		}

		public PluginsPage.test LoadScript(string Path, string name)
		{
			if (loadedScriptNamesIdentifier.Contains(name))
				return null;
			var skript = new Script(Path, name, this);
			foreach(var variable in variables.Keys)
			{
				skript.addVar(variable, variables[variable]);
			}
			skript.setup();
			loadedScriptNamesIdentifier.Add(name);
			loadedScriptNamesVisual.Add(skript.getName());
			loadedScripts.Add(skript);
		    return new PluginsPage.test
		    {
		        
		    };
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

        public void CallFunctions(string funcName, params object[] pars)
        {
            foreach (var skript in loadedScripts)
            {
                skript.callFunc(funcName, pars);
            }
        }
    }
}
