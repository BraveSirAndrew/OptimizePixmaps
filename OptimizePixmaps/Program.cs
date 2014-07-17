using System;
using Duality;
using Duality.Resources;

namespace OptimizePixmaps
{
	public class Program
	{
		public static void Main()
		{
			Console.WriteLine("Optimizing pixmaps...");

			var pixmapResources = Resource.GetResourceFiles("Data\\Pixmaps");
			foreach (var pixmapResource in pixmapResources)
			{
				if (pixmapResource.EndsWith(".Pixmap.res", StringComparison.CurrentCultureIgnoreCase) == false)
					continue;

				var pixmap = ContentProvider.RequestContent<Pixmap>(pixmapResource);

				if (pixmap == null)
					continue;

				pixmap.Res.PixelData[0].ClearPixelData();
				pixmap.Res.Save();

				ContentProvider.ClearContent();
			}

			Console.WriteLine("Finished");
		}
	}
}