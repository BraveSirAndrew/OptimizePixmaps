using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Duality;
using Duality.Resources;
using Duality.Serialization;
using HonourBound.Resources;
using UtilsAndResources;

namespace BuildPreloaders
{
	class Program
	{
		private static string _gamePath;

		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

			_gamePath = null;
			if (args.Length > 0)
				_gamePath = args[0];
			if (!DirectoryHelper.ExistsAndPathValid(_gamePath))
				Environment.Exit(-1);

			Assembly.LoadFrom(Path.Combine(_gamePath, "Mercury.ParticleEngine.dll"));
			Assembly.LoadFrom(Path.Combine(_gamePath, "sharppaint.dll"));		// not sure why we need this but we do :(
			Assembly.LoadFrom(Path.Combine(_gamePath, "Plugins\\ScriptingPlugin.core.dll"));
			Assembly.LoadFrom(Path.Combine(_gamePath, "Plugins\\ScriptingCSCorePlugin.core.dll"));
			Assembly.LoadFrom(Path.Combine(_gamePath, "Plugins\\ScriptingFSCorePlugin.core.dll"));

			var success = false;
			try
			{
				success = PackLevels();
			}
			catch (Exception e)
			{
				Console.WriteLine("Something went wrong while building preloaders. The error was:\n{0}", e.Message);
				Environment.Exit(-1);
			}

			if (success == false)
				Environment.Exit(-1);
		}

		private static bool PackLevels()
		{
			Resource.BlockAllInits = true;
			
			DualityApp.Init(DualityApp.ExecutionEnvironment.Launcher);
			Formatter.DefaultMethod = FormattingMethod.Binary;

			var scenes = new List<string>
			{
				"samurai-village",
				"imafuku",
				"tengu-city",
				"hirumos-return",
				"hirumos-return-boss",
				"earth-clan-city",
				"Yomi",
				"yomi-castle"
			};

			foreach (var sceneName in scenes)
			{
				var resourcesUsedByScene = new HashSet<IContentRef>();
				var scene = ContentProvider.RequestContent<Scene>(Path.Combine(_gamePath, "Data\\Scenes", sceneName + ".Scene.res"));
				if (scene.Res == null)
				{
					Console.WriteLine("Couldn't load scene '{0}'", sceneName);
				}
				ReflectionHelper.VisitObjectsDeep(scene.Res, FindContentRefs(resourcesUsedByScene), false);

				var packResource = new PackResource();
				foreach (var contentRef in resourcesUsedByScene)
				{
					// don't include render targets, or we might end up reloading render target textures while the preloader is using them,
					// which makes everything turn white!
					if (contentRef.FullName.Contains("RenderTargets"))
						continue;

					using (var stream = new MemoryStream())
					{
						contentRef.Res.Save(stream);
						packResource.AddResource(contentRef.Path, stream.ToArray());
					}
				}
				packResource.Save(Path.Combine(_gamePath, "Data\\Scenes", scene.Name + ".PackResource.res"));
			}

			DualityApp.Terminate();

			return true;
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (args.Name.ToLower().Contains("gameplugin.core"))
				return Assembly.LoadFrom(Path.Combine(_gamePath, "Plugins\\GamePlugin.core.dll"));

			return null;
		}

		private static Func<IContentRef, IContentRef> FindContentRefs(HashSet<IContentRef> resourcesUsedByScene)
		{
			return r =>
			{
				if (r.Res == null)
					return r;

				if (resourcesUsedByScene.Contains(r))
					return r;

				Log.Editor.Write("Adding '{0}'", r.Res.Name);
				resourcesUsedByScene.Add(r);

				if (r.Is<Prefab>())
				{
					try
					{
						var instance = ((Prefab)r.Res).Instantiate();
						ReflectionHelper.VisitObjectsDeep(instance, FindContentRefs(resourcesUsedByScene), false);
					}
					catch (Exception e)
					{
						Log.Editor.WriteError("Failed to load prefab '{0}'", r.Res);
					}
				}
				return r;
			};
		}
	}
}
