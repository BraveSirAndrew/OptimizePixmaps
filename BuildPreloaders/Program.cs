using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using Duality;
using Duality.Resources;
using Duality.Serialization;
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
				"front-end",
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

				var preloadFile = Path.Combine(_gamePath, "preloaders", sceneName + "-preload-info.txt");
				if (File.Exists(preloadFile) == false)
				{
					Console.WriteLine("Couldn't find a preloader file for " + sceneName + ". No preloader will be available for that level.");
					continue;
				}

				var resources = File.ReadAllText(preloadFile).Split(new []{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
				foreach (var resource in resources)
				{
					var resName = resource.ToLower();
					if (resName.Contains("rendertargets") ||
						resName.Contains("playerprogressionresource") ||
						resName.Contains("unlockablemove") ||
						resName.Contains("Data\\Scripts"))
						continue;
					
					resourcesUsedByScene.Add(ContentProvider.RequestContent(resource));
				}

				using(var fileStream = new FileStream(Path.Combine(_gamePath, "levels\\", sceneName + ".pack"), FileMode.Create))
				using (var zipArchive = new ZipArchive(fileStream, ZipArchiveMode.Create))
				using(var texFileStream = new FileStream(Path.Combine(_gamePath, "levels\\", sceneName + ".tex"), FileMode.Create))
				using (var textureZipArchive = new ZipArchive(texFileStream, ZipArchiveMode.Create))
				{
					foreach (var contentRef in resourcesUsedByScene.OrderBy(OrderByType))
					{
						if (contentRef.IsAvailable == false)
						{
							Log.Game.WriteWarning("{0} could not be loaded. It's possible the resource is listed in the preloader file but has been deleted. Skipping...", contentRef);
							continue;
						}

						if (contentRef.Is<Pixmap>())
							continue;

						if (contentRef.Is<Texture>() && contentRef.Name.ToLower().Contains("rendertarget") == false)
						{
							var entry = textureZipArchive.CreateEntry(contentRef.Path);
							var texture = contentRef.As<Texture>().Res;
							using (var entryStream = entry.Open())
							{
								var pixmap = texture.BasePixmap.Res;

								entryStream.Write(BitConverter.GetBytes(pixmap.ProcessedLayer.Width), 0, 4);
								entryStream.Write(BitConverter.GetBytes(pixmap.ProcessedLayer.Height), 0, 4);

								if (pixmap.ProcessedLayer.IsCompressed)
									entryStream.Write(pixmap.ProcessedLayer.CompressedData, 0, pixmap.ProcessedLayer.CompressedImageSize);
								else
									entryStream.Write(pixmap.ProcessedLayer.GetPixelDataByteRgba(), 0, pixmap.ProcessedLayer.ImageSize);
							}

							texture.UseExternalPixelData = true;
						}

						using (var stream = new MemoryStream())
						{
							contentRef.Res.Save(stream);
							stream.Seek(0, SeekOrigin.Begin);

							var entry = zipArchive.CreateEntry(contentRef.Path);
							using (var entryStream = entry.Open())
							{
								var buffer = stream.ToArray();
								entryStream.Write(buffer, 0, buffer.Length);
							}
						}
					}
				}
			}

			DualityApp.Terminate();

			return true;
		}

		/// <summary>
		/// Make sure textures are loaded first
		/// </summary>
		/// <param name="arg"></param>
		/// <returns></returns>
		private static object OrderByType(IContentRef arg)
		{
			if (arg.Is<Texture>())
				return 0;

			return 1;
		}

		private static Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (args.Name.ToLower().Contains("gameplugin.core"))
				return Assembly.LoadFrom(Path.Combine(_gamePath, "Plugins\\GamePlugin.core.dll"));

			return null;
		}
	}
}
