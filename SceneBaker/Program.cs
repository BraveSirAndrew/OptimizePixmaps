using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Duality;
using Duality.Resources;
using HonourBound.Editor.LayerEditorTools;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace SceneBaker
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				
				var sw = Stopwatch.StartNew();
				Console.WriteLine("In SceneBaker started process");
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

				if(Log.Editor.Outputs.Any(o => o is ConsoleLogOutput) == false)
					Log.Editor.AddOutput(new ConsoleLogOutput());

				if (Log.Core.Outputs.Any(o => o is ConsoleLogOutput) == false)
					Log.Core.AddOutput(new ConsoleLogOutput());

				if (Log.Game.Outputs.Any(o => o is ConsoleLogOutput) == false)
					Log.Game.AddOutput(new ConsoleLogOutput());
				
				DualityApp.Init(DualityApp.ExecutionEnvironment.Launcher, DualityApp.ExecutionContext.Editor);
				
				using (var launcherWindow = new HeadlessWindow(
					DualityApp.UserData.GfxWidth,
					DualityApp.UserData.GfxHeight,
					DualityApp.DefaultMode,
					DualityApp.AppData.AppName,
					GameWindowFlags.Default))
				{
					// Initialize default content
					launcherWindow.MakeCurrent();

					Log.Core.Write("OpenGL initialized");
					Log.Core.PushIndent();
					Log.Editor.Write("Vendor: {0}", GL.GetString(StringName.Vendor));
					Log.Editor.Write("Version: {0}", GL.GetString(StringName.Version));
					Log.Editor.Write("Renderer: {0}", GL.GetString(StringName.Renderer));
					Log.Editor.Write("Shading language version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
					Log.Core.PopIndent();
				
					DualityApp.TargetResolution = new Vector2(launcherWindow.Width, launcherWindow.Height);
					DualityApp.TargetMode = launcherWindow.Context.GraphicsMode;
					ContentProvider.InitDefaultContent();

					Bake();
				}
				DualityApp.Terminate();

				Console.WriteLine("All levels baked in " + sw.Elapsed.TotalSeconds + " seconds. Delicious!");
			}
			catch (Exception exception)
			{
				Console.WriteLine("An error ocurred: {0} {1} {2}", exception.Message, Environment.NewLine, exception.StackTrace);
				Environment.Exit(-1);
			}
		}

		private static void Bake()
		{
			var renderTexture = new Texture(2048, 2048, filterMin: TextureMinFilter.Linear);
			var renderTarget = new RenderTarget(AAQuality.Off, new[] {new ContentRef<Texture>(renderTexture)});
			var scenes = ContentProvider.GetAvailableContent(typeof (Scene))
				.Where(s => Path.GetDirectoryName(s.Path) == "Data\\Scenes")
				.Select(s => (Scene) s.Res);

			foreach (var scene in scenes)
			{
				TileSystemSceneCruncher.CrunchScene(scene, renderTarget, renderTexture);
			}

			renderTexture.Dispose();
			renderTarget.Dispose();
		}
	}

	public class HeadlessWindow : GameWindow
	{
		public HeadlessWindow(int w, int h, GraphicsMode mode, string title, GameWindowFlags flags) : base(w, h, mode, title, flags) { }
	}
}
