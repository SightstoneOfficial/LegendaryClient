using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;

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
				return e;
			}
		}
	}
}
