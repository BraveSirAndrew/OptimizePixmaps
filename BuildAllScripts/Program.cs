using System;
using System.IO;
using Duality;

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
			if (string.IsNullOrWhiteSpace(gamePath))
			{
				Console.WriteLine(@"Please include the game path as an argument to this program like BuildAllScripts.exe c:\path\to\game");
				return;
			}
			if (!Directory.Exists(gamePath))
			{
				Console.WriteLine("Directory {0} does not exists. Can't compile scripts", gamePath);
				return;
			}
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
