using System;
using Duality;
using ScriptingPlugin.Resources;

namespace BuildAllScripts
{
	class Program
	{
		static void Main(string[] args)
		{
			const string folderPath = "Data\\Scripts";
			var scriptResources = Resource.GetResourceFiles(folderPath);
		
			Fuck.Fucjk(scriptResources.ToArray());
		}
	}

	public class Fuck
	{
		public static void Fucjk(string[] scripts)
		{
			Console.WriteLine("Optimizing pixmaps...");
			if (scripts == null || scripts.Length == 0)
			{
				Console.WriteLine("Found 0 pixmaps.");
				Console.WriteLine("Finished.");
				return;
			}
			try
			{

				foreach (var scriptPath in scripts)
				{
					if (scriptPath.EndsWith("Script.res", StringComparison.CurrentCultureIgnoreCase) == false)
						continue;
					

					if (scriptPath.EndsWith("FSharpScript.res"))
					{
						var scriptResource = ContentProvider.RequestContent<FSharpScript>(scriptPath);
						if (scriptResource == null || scriptResource.Res == null)
							continue;
//						scriptResource.Res.Script
//						scriptResource.Res.SourcePath

					}
					if (scriptPath.EndsWith("CSharpScript.res"))
					{
						var scriptResource = ContentProvider.RequestContent<CSharpScript>(scriptPath);

					}
					


				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("An error ocurred: {0} {1} {2}", exception.Message, Environment.NewLine, exception.StackTrace);
			}
			Console.WriteLine("Finished building scripts");

		}
	}
}
