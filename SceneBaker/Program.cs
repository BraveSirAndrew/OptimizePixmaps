using System.Linq;
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
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

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
