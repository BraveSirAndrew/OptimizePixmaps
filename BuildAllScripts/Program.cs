using System;
using System.IO;
using Duality;
using UtilsAndResources;

namespace BuildAllScripts
{
	class Program
	{
		static void Main(string[] args)
		{
			const string scriptsRelativePath = "Data\\Scripts";
			string gamePath = null;
			if (args.Length > 0)
				gamePath = args[0];
			if(!DirectoryHelper.ExistsAndPathValid(gamePath) )
				return;
			var scriptsCompletePath = Path.Combine(gamePath, scriptsRelativePath);
			if (!Directory.Exists(scriptsCompletePath))
			{
				Console.WriteLine("Scripts resources Directory {0} does not exists. Can't compile scripts", scriptsCompletePath);
				return;
			}

			try
			{
				var scriptResources = Resource.GetResourceFiles(scriptsCompletePath);

				ScriptResourcesBuilder.BuildAllScripts(scriptResources.ToArray(), gamePath);
			}
			catch (Exception exception)
			{
				Console.WriteLine("An error ocurred: {0} {1} {2}", exception.Message, Environment.NewLine, exception.StackTrace);
			}
		}
	}
}
