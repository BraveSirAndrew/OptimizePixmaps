using System.IO;
using System.Linq;
using Duality;
using Duality.Resources;
using NUnit.Framework;

namespace OptimizePixmaps.Tests
{
	[TestFixture]
    public class OptimizePixmapsTests
    {
		[Test]
		public void RemovesFirstLayerOfAllPixmapsInPixmapsFolder()
		{
			if (Directory.Exists("Data\\Pixmaps") == false)
			{
				Directory.CreateDirectory("Data\\Pixmaps");
			}

			var layer = new Pixmap.Layer();
			layer.SetPixelDataRgba(Enumerable.Repeat(MathF.Rnd.NextByte(), 400).ToArray(), 10, 10);
			var pixmap = new Pixmap(layer);
			pixmap.PixelData.Add(layer);
			pixmap.Save("Data\\Pixmaps\\PixmapOne.pixmap.res");

			Program.Main();

			Assert.AreEqual(0, layer.Data.Length);
		}

		[Test]
		public void DoesntRemoveSubsequentLayers()
		{
			if (Directory.Exists("Data\\Pixmaps") == false)
			{
				Directory.CreateDirectory("Data\\Pixmaps");
			}

			if(File.Exists("Data\\Pixmaps\\PixmapOne.pixmap.res"))
				File.Delete("Data\\Pixmaps\\PixmapOne.pixmap.res");

			var layer = new Pixmap.Layer();
			layer.SetPixelDataRgba(Enumerable.Repeat(MathF.Rnd.NextByte(), 400).ToArray(), 10, 10);
			var pixmap = new Pixmap(layer);
			pixmap.PixelData.Add(layer);

			var layer2 = new Pixmap.Layer();
			layer2.SetPixelDataRgba(Enumerable.Repeat(MathF.Rnd.NextByte(), 400).ToArray(), 10, 10);
			pixmap.PixelData.Add(layer2);

			pixmap.Save("Data\\Pixmaps\\PixmapOne.pixmap.res");

			Program.Main();

			Assert.AreEqual(100, layer2.Data.Length);
		}
    }
}
