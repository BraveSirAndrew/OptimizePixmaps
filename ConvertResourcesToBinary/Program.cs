﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
			try
			{
				//LoadAllPlugins(_directoryPath);
				var results = SerializationConverter.ConvertToBinary();
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

                plugins = plugins.Except(new[] { "spine-csharp", "nvorbis", "CommunityExpress" }, StringComparer.CurrentCultureIgnoreCase);
				var references = plugins.Where(x => !x.ToLower().Contains("fmod")).ToList();
				foreach (var reference in references)
				{
					try
					{
                        Console.WriteLine("Loading assembly {0}", reference);
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
	
	}

	public static class SerializationConverter
	{
		public static Results ConvertToBinary()
		{
			var errors = new List<string>();

			Resource.BlockAllInits = true;
			Resource.CompressOnSave = true;

			DualityApp.Init(DualityApp.ExecutionEnvironment.Launcher);
			Formatter.DefaultMethod = FormattingMethod.Binary;

			DualityApp.LoadAppData();
			DualityApp.LoadUserData();
			DualityApp.LoadMetaData();

			DualityApp.SaveAppData();
			DualityApp.SaveUserData();
			DualityApp.SaveMetaData();

			var resFiles = Resource.GetResourceFiles();
			foreach (var file in resFiles)
			{
				try
				{
					var contentRef = Resource.Load<Resource>(file, null, false);
					Console.WriteLine("Converting {0}",file);
					contentRef.Save(null, false);
					contentRef.Dispose();
				}
				catch (Exception exception)
				{
					errors.Add(string.Format("Error saving {0}:{1} {2} StackTrace: {3}",file, exception.Message, Environment.NewLine, exception.StackTrace));
				}
			}

			DualityApp.Terminate();
			return new Results {Succeded = !errors.Any(), Errors = errors.ToArray()};
		}
	}
}
