using System;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;
using System.IO;
using LegendaryClient.Logic.MultiUser;

namespace LegendaryClient.Scripting_Environment
{
	
	class Interpreter
	{
		private ScriptEngine pyEngine = null;
		private ScriptRuntime pyRuntime = null;
		private ScriptScope pyScope = null;
		private CompiledCode code = null;
		private bool isLoaded = false;
		public Interpreter()
		{
			pyEngine = Python.CreateEngine();
			pyRuntime = pyEngine.Runtime;
			pyScope = pyEngine.CreateScope();
			var paths = pyEngine.GetSearchPaths();
			paths.Add(Path.Combine(Client.ExecutingDirectory, "Client", "LIB"));
			pyEngine.SetSearchPaths(paths);
		}

		public object loadCode(string Path)
		{
			try
			{
				code = pyEngine.CreateScriptSourceFromFile(Path).Compile();
				return true;
			}
			catch (Exception e)
			{
				return e;
			}
		}

		public object runCode()
		{
			try
			{
				code.Execute(pyScope);
				return true;
			}
			catch(Exception e)
			{
				return e;
			}
			
        }

		public void addVar(string varName, object value)
		{

			pyScope.SetVariable(varName, value);
		}

		public dynamic getValue(string name)
		{
			try
			{
				return pyScope.GetVariable(name);
			}
			catch(Exception e)
			{
				return e.Message;
			}
		}
	}
}
