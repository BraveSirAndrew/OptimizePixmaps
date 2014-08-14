using System;
using System.IO;
using System.Reflection;
using Duality;
using UtilsAndResources;

namespace BuildAllScripts
{
	class Program
	{
		private static string _gamePath;

		static void Main(string[] args)
		{
			Console.WriteLine("test");
			Console.ReadLine();
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
			const string scriptsRelativePath = "Data\\Scripts";
			
			_gamePath = null;
			if (args.Length > 0)
				gamePath = args[0];
			if(!DirectoryHelper.ExistsAndPathValid(gamePath) )
				return;
			var scriptsCompletePath = Path.Combine(_gamePath, scriptsRelativePath);
			if (!Directory.Exists(scriptsCompletePath))
			{
				Console.WriteLine("Scripts resources Directory {0} does not exists. Can't compile scripts", scriptsCompletePath);
				return;
			}

			try
			{
				var scriptResources = Resource.GetResourceFiles(scriptsCompletePath);
				var scriptBuildingResult = new ScriptResourcesBuilder().BuildAllScripts(scriptResources.ToArray(), _gamePath);
				if (scriptBuildingResult.Item1)
				{
					Console.WriteLine("Finished building scripts");
				}
				else
				{
					Environment.Exit(-1);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("An error ocurred: {0} {1} {2}", exception.Message, Environment.NewLine, exception.StackTrace);
				Environment.ExitCode = -1;
			}
		}




		private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{

			const string scriptingdll = "ScriptingPlugin.core";
			var pluginPath = Path.Combine(_gamePath, "plugins", scriptingdll + ".dll");
			if (!File.Exists(pluginPath))
			{
				Console.WriteLine("Can't find Core Plugin, abort Build all scripts");
				throw new ArgumentException(string.Format("{0} not found, cant reset resources", pluginPath));
			}

			return args.Name.ToLower().Contains(scriptingdll) ? Assembly.LoadFile(pluginPath) : null;
		}
	}
}
