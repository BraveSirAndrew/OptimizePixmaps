using System;
using System.IO;

namespace UtilsAndResources
{
    public class DirectoryHelper
    {
	    public static void DeleteDirectoryContents(string targetPath, bool deleteRoot = true)
		{
			if (!Directory.Exists(targetPath))
				return;
			string[] files = Directory.GetFiles(targetPath);
			string[] dirs = Directory.GetDirectories(targetPath);

			foreach (string file in files)
			{
				File.SetAttributes(file, FileAttributes.Normal);
				File.Delete(file);
				Console.Write("Deleted file {0}", file);
			}

			foreach (string dir in dirs)
			{
				DeleteDirectoryContents(dir);
				Console.Write("Deleted directory {0}", dir);
			}
			if (deleteRoot)
				Directory.Delete(targetPath, false);
		}

	    public static bool ExistsAndPathValid(string directoryPath)
	    {
			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				Console.WriteLine(@"Please include the game path as an argument to this program like BuildAllScripts.exe c:\path\to\game");
				return false;
			}
			if (!Directory.Exists(directoryPath))
			{
				Console.WriteLine("Directory {0} does not exists. Can't compile scripts", directoryPath);
				return false;
			}
			return true;
	    }
    }
}
