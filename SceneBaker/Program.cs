﻿using System;
using System.Diagnostics;
using System.Globalization;
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

				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

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
			var scenes = ContentProvider.GetAvailableContent(typeof (Scene)).Select(s => (Scene) s.Res);

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
