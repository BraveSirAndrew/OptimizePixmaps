using Duality;

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
}