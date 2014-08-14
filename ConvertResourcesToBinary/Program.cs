using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Duality;
using Duality.Serialization;
using UtilsAndResources;

namespace ConvertResourcesToBinary
{
	class Program
	{
		static void Main(string[] args)
		{
			var directoryPath = args[0];
			if(!DirectoryHelper.ExistsAndPathValid(directoryPath))
				return;
			var dataPath = Path.Combine(directoryPath, "Data");
			if (!Directory.Exists(dataPath))
			{
				Console.WriteLine("Resources Directory {0} does not exists. Can't compile scripts", dataPath);
				return;
			}
			try
			{
				var results = SerializationConverter.ConvertToBinary(dataPath);
				if (results.Succeded)
					Console.WriteLine("Converted all resources to binary");
				else
				{
					Console.WriteLine("There were errors saving resources as binaries");
					Console.WriteLine("Errors: {0}", string.Join(Environment.NewLine, results.Errors));
					Environment.Exit(-1);
				}
			}
			catch (Exception exception)
			{
				Console.WriteLine("An error ocurred: {0} {1} {2}", exception.Message, Environment.NewLine, exception.StackTrace);
				Environment.Exit(-1);
			}
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
					var res = Resource.Load<Resource>(file, null, false);
					res.Save(null, false);
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
