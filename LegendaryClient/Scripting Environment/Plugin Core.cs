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
		private ScriptEngine pyEngine = null;
		private ScriptRuntime pyRuntime = null;
		private ScriptScope pyScope = null;
		public Plugin_Core()
		{
			pyEngine = Python.CreateEngine();
			//actually load the STD lib
			ICollection<string> searchPaths = pyEngine.GetSearchPaths();
			searchPaths.Add("..\\..\\Lib");
			pyEngine.SetSearchPaths(searchPaths);
			pyRuntime = Python.CreateRuntime();
			pyScope = pyEngine.CreateScope();
			
		}
		
		/// <summary>
		/// Add a variable to the Interpreter Scope
		/// </summary>
		/// <param name="varName"></param>
		/// <param name="value"></param>
		public void addVar(string varName, object value){
			pyScope.SetVariable(varName,value);
		}
		/// <summary>
		/// Add a variable to the Interpreter Scope
		/// </summary>
		/// <param name="varName"></param>
		/// <param name="handle"></param>
		public void addVar(string varName, System.Runtime.Remoting.ObjectHandle handle)
		{
			pyScope.SetVariable(varName, handle);
		}
		/// <summary>
		/// Load a Plugin from a specified Path
		/// </summary>
		/// <param name="Path"></param>
		/// <returns>The compiled Code</returns>
		public CompiledCode loadScriptFromFile(string Path)
		{
			var script = pyEngine.CreateScriptSourceFromFile(Path);
			var compiled = script.Compile();
			return compiled;
		}
		/// <summary>
		/// Run the compiled Code
		/// </summary>
		/// <param name="code"></param>
		public void runScript(CompiledCode code){
			code.Execute(pyScope);
		}
	}
}
