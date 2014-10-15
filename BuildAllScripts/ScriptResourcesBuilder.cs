using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Duality;
using ScriptingPlugin;
using ScriptingPlugin.Resources;
using UtilsAndResources;

namespace BuildAllScripts
{
	
	public  class ScriptResourcesBuilder
	{
		public Results BuildAllScripts(string[] scripts, string gamePath)
		{
			Console.WriteLine("Compiling all scripts...");

			if (scripts == null || scripts.Length == 0)
			{
				Console.WriteLine("Found 0 scripts.");
				Console.WriteLine("Finished.");
				return new Results(false,new[]{"found 0 scripts"});
			}
			Console.WriteLine("About to start actually compiling {0} scripts.", scripts.Length);

			var resultingAssemblyDirectory = Path.Combine(gamePath, "Scripts");
			DirectoryHelper.DeleteDirectoryContents(resultingAssemblyDirectory, false);

			var cSharpResults = ScriptCompiler<CSharpScript, CSharpScriptCompiler>(scripts, gamePath, "CSharpScript.res", resultingAssemblyDirectory);
			if (cSharpResults.Errors.Any())
			{
				Console.WriteLine("There were errors found when compiling the scripts");
				Console.WriteLine(string.Join(Environment.NewLine, cSharpResults.Errors));
				return  new Results(false,cSharpResults.Errors.ToArray());

			}
			Console.WriteLine("All C# scripts compiled without error");
			var fSharpResults = ScriptCompiler<FSharpScript, FSharpScriptCompiler>(scripts, gamePath, "FSharpScript.res", resultingAssemblyDirectory);
			if (fSharpResults.Errors.Any())
			{
				Console.WriteLine("There were errors found when compiling the scripts");
				Console.WriteLine(string.Join(Environment.NewLine, fSharpResults.Errors));
				return  new Results(false,fSharpResults.Errors.ToArray());

			}
			Console.WriteLine("All F# scripts compiled without error");
			
			return  new Results(true,null);
		}
	
		private  IScriptCompilerResults ScriptCompiler<TResource, TCompiler>(IEnumerable<string> scripts, string gamePath, string extension, string resultingAssemblyDirectory)
			where TResource : ScriptResourceBase
			where TCompiler : IScriptCompiler, new()
		{
		    if(scripts== null || !scripts.Any())
		        return null;

		    var scriptsForCompiling = new List<CompilationUnit>();

		    foreach (var scriptPath in scripts)
			{
				if (scriptPath.EndsWith("Script.res", StringComparison.CurrentCultureIgnoreCase) == false)
					continue;


				if (scriptPath.EndsWith(extension))
				{
					var scriptResource = ContentProvider.RequestContent<TResource>(scriptPath);
					if (scriptResource == null || scriptResource.Res == null)
						continue;
					scriptsForCompiling.Add(new CompilationUnit(scriptResource.Res.Script, scriptResource.Res.SourcePath));
				}
			}
            if(!scriptsForCompiling.Any())
                return new FSharpScriptCompilerResults(new string[0],null, null);
			var compiler = CreateCompilerWithReferences<TCompiler>(gamePath);

			return compiler.Compile(scriptsForCompiling, resultingAssemblyDirectory);

		}

		private IScriptCompiler CreateCompilerWithReferences<T>(string gamePath) where T : IScriptCompiler, new()
		{
			var scriptCompiler = new T();
			var di = new DirectoryInfo(gamePath);
			var fis = di.GetFiles("*.dll", SearchOption.TopDirectoryOnly).ToList();
			fis.Add(new FileInfo("onikira.exe"));
			var plugins = new DirectoryInfo(Path.Combine(gamePath, "plugins"));
			fis.AddRange(plugins.GetFiles("*.dll", SearchOption.TopDirectoryOnly));
			var spine = fis.FirstOrDefault(x => x.Name.ToLower().Contains("spine-csharp"));
			var nvorbis = fis.FirstOrDefault(x => x.Name.ToLower().Contains("nvorbis"));
			fis.Remove(spine);
			fis.Remove(nvorbis);
			var references = fis.Where(x => (!x.Name.ToLower().Contains("openal") &&
											 !x.Name.ToLower().Contains("steam") &&
											 !x.Name.ToLower().Contains("sdl2") &&
											 !x.Name.ToLower().Contains("fmod") &&
											 !x.Name.ToLower().Contains("nativesquish") &&
											 !x.Name.ToLower().Contains("communityexpresssw") &&
											 !x.Name.ToLower().Contains("oggvorbisdotnet") &&
											 !x.Name.ToLower().Contains("libvorbis") &&
											 !x.Name.ToLower().Contains("libtheoraplay") &&
											 !x.Name.ToLower().Contains("libtheora") &&
											 !x.Name.ToLower().Contains("libogg") &&
											 !x.Name.ToLower().Contains("wrap_oal"))).ToList();

            foreach (FileInfo fileInfo in references)
            {
                if (!(scriptCompiler is FSharpScriptCompiler && (fileInfo.Name.EndsWith("System.Runtime.dll", StringComparison.CurrentCultureIgnoreCase)
                                                             || fileInfo.Name.EndsWith("onikira.exe", StringComparison.CurrentCultureIgnoreCase))))
                          scriptCompiler.AddReference(fileInfo.FullName);
            }
			HorribleHackToLoadGACAssembloes();
			var referenceAssembliesFile = Path.Combine(gamePath, "ScriptReferences.txt");
			if (File.Exists(referenceAssembliesFile))
			{
				var assemblies = File.ReadAllText(referenceAssembliesFile).Split('\n');

				foreach (var assembly in assemblies)
				{
					if (assembly.ToLower().StartsWith("plugins") || references.Any(x => assembly.ToLower().Contains(x.Name.ToLower())))
						continue;
                    if (!(scriptCompiler is FSharpScriptCompiler && (assembly.EndsWith("System.Runtime.dll",StringComparison.CurrentCultureIgnoreCase) || assembly.StartsWith("onikira.exe",StringComparison.CurrentCultureIgnoreCase))))
					    scriptCompiler.AddReference(assembly);
				}
			}
			return scriptCompiler;
		}

		private void HorribleHackToLoadGACAssembloes() 
		{
			var b = new Bitmap(1, 1);
			var ssse = new XDocument();
            IEnumerable<int> bla = new List<int>();
            IEnumerator<object> blu ;
		}
	}
}