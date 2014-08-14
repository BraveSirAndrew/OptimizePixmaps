using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Duality;
using Duality.Serialization;
using UtilsAndResources;

namespace ConvertResourcesToBinary
{
	class Program
	{
		private static string _directoryPath;

		static void Main(string[] args)
		{
			_directoryPath = args[0];
			if(!DirectoryHelper.ExistsAndPathValid(_directoryPath))
				return;
			var dataPath = Path.Combine(_directoryPath, "Data");
			if (!Directory.Exists(dataPath))
			{
				Console.WriteLine("Resources Directory {0} does not exists. Can't compile scripts", dataPath);
				return;
			}
			try
			{
				LoadAllPlugins(_directoryPath);
				var results = SerializationConverter.ConvertToBinary(dataPath);
				if (results.Succeded)
					Console.WriteLine("Converted all resources to binary");
				else
				{
					Console.WriteLine("There were {0} errors saving resources as binaries", results.Errors.Length);
					Console.WriteLine("Error: {0}", string.Join(Environment.NewLine, results.Errors));
					Environment.Exit(-1);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("An error ocurred: {0} {1} {2}", exception.Message, Environment.NewLine, exception.StackTrace);
				Environment.Exit(-1);
			}
		}

		private static void LoadAllPlugins(string gamePath)
		{
			try
			{
				var plugins = Directory.EnumerateFiles(Path.Combine(gamePath, "plugins"),"*.dll");

				plugins = plugins.Except(new []{"spine-csharp","nvorbis"}, StringComparer.CurrentCultureIgnoreCase);
				var references = plugins.Where(x => (!x.ToLower().Contains("fmod"))).ToList();
			//	HorribleHackToLoadGACAssembloes();
				foreach (var reference in references)
				{
					try
					{
						Assembly.LoadFile(reference);
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("Error: {0}.{1} {2} ",exception.Message, Environment.NewLine, exception.StackTrace);
			}
		}
		private static void HorribleHackToLoadGACAssembloes()
		{
			var b = new Bitmap(1, 1);
			var ssse = new XDocument();
		}
	
	}

	public static class SerializationConverter
	{
		public static Results ConvertToBinary(string path)
		{
			Formatter.DefaultMethod = FormattingMethod.Binary;
			var errors = new List<string>();
			
			var resFiles = Resource.GetResourceFiles(path);
			foreach (var file in resFiles)
			{
				try
				{
					var contentRef = Resource.Load<Resource>(file,null,false);
//					var contentRef = ContentProvider.RequestContent<Resource>(file);
//					var contentRef = ContentProvider.RequestContent(file);
					Console.WriteLine("Converting {0}",file);
					contentRef.Save(null, false);
				}
				catch (Exception exception)
				{
					errors.Add(string.Format("Error:{0} {1} StackTrace: {2}", exception.Message, Environment.NewLine, exception.StackTrace));
				}
			}
			return new Results {Succeded = !errors.Any(), Errors = errors.ToArray()};
		}
	}
}
