using System;
using System.Collections.Generic;
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
			CreateDirectory();
			var layer = new Pixmap.Layer();
			var pixmap = CreatePixmapWithOneLayer(layer);
			pixmap.Save("Data\\Pixmaps\\PixmapOne.pixmap.res");

			Program.Main();

			Assert.AreEqual(0, layer.Data.Length);
		}

		[Test]
		public void DoesntRemoveSubsequentLayers()
		{
			CreateDirectory();
			var pixmap = CreatePixmapWithOneLayer(new Pixmap.Layer());
			var layer2 = new Pixmap.Layer();
			layer2.SetPixelDataRgba(Enumerable.Repeat(MathF.Rnd.NextByte(), 400).ToArray(), 10, 10);
			pixmap.PixelData.Add(layer2);
			const string path = "Data\\Pixmaps\\PixmapOne.pixmap.res";
			pixmap.Save(path);

			Program.Main();

			var loadedPixmap = ContentProvider.RequestContent<Pixmap>(path).Res;
			Assert.AreEqual(400, loadedPixmap.PixelData[1].ImageSize);
		}

		[Test]
		public void Does_not_throw_on_null_list()
		{
			Assert.DoesNotThrow(() => PixmapOptimizer.Optimize(null));
		}

		[Test]
		public void When_name_doesnt_contain_pixmap_extenstion_then_not_processed()
		{
			Assert.DoesNotThrow(()=> PixmapOptimizer.Optimize(new List<string>{"asdasd"})); //no ideal as a way to verify but...
		}

		[Test]
		public void When_pixmap_extenstion_but_resouce_doesnt_exist_Then_not_processed()
		{
			Assert.DoesNotThrow(() => PixmapOptimizer.Optimize(new List<string> { "asdasd.Pixmap.res" })); //not ideal as a way to verify but...
		}

		private static Pixmap CreatePixmapWithOneLayer(Pixmap.Layer layer)
		{
			if (File.Exists("Data\\Pixmaps\\PixmapOne.pixmap.res"))
				File.Delete("Data\\Pixmaps\\PixmapOne.pixmap.res");

			layer.SetPixelDataRgba(Enumerable.Repeat(MathF.Rnd.NextByte(), 400).ToArray(), 10, 10);
			var pixmap = new Pixmap(layer);
			
			return pixmap;
		}

		private static void CreateDirectory()
		{
			if (Directory.Exists("Data\\Pixmaps") == false)
			{
				Directory.CreateDirectory("Data\\Pixmaps");
			}
		}
    }
}
