using System;
using System.Collections.Generic;
using Duality;
using Duality.Resources;

namespace OptimizePixmaps
{
	public class Program
	{
		public static void Main()
		{
			const string folderPath = "Data\\Pixmaps";
			var pixmapResources = Resource.GetResourceFiles(folderPath);
		
			PixmapOptimizer.Optimize(pixmapResources);
		}
	}

	public class PixmapOptimizer
	{
		public static void Optimize(IEnumerable<string> pixmapResources)
		{
			Console.WriteLine("Optimizing pixmaps...");
			if (pixmapResources == null)
			{
				Console.WriteLine("Found 0 pixmaps.");
				Console.WriteLine("Finished.");
				return;
			}
			try
			{
				foreach (var pixmapResource in pixmapResources)
				{
					if (pixmapResource.EndsWith(".Pixmap.res", StringComparison.CurrentCultureIgnoreCase) == false)
						continue;

					try
					{
						var pixmap = ContentProvider.RequestContent<Pixmap>(pixmapResource);

						if (pixmap == null || pixmap.Res == null)
							continue;

						pixmap.Res.PixelData[0].ClearPixelData();
						pixmap.Res.Save();

						ContentProvider.ClearContent();
					}
					catch (Exception exception)
					{
						Console.WriteLine("An error occurred processing {0}. Error {1} {2} {3} will proceed with next file", pixmapResource,exception.Message, Environment.NewLine, exception.StackTrace);
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("An error ocurred: {0} {1} {2}",exception.Message, Environment.NewLine, exception.StackTrace);
			}

			Console.WriteLine("Finished optimizing pixmaps");
		}
	}
}