using System;
using System.IO;
using System.Reflection;
using Duality;
using HonourBound.PlatformIntegration;
using HonourBound.Resources;

namespace CleanResources
{
	public class Program
	{
		private static string _pathToGame;

		/// <summary>
		/// Resets the local leaderboard and achievement as well as 
		/// </summary>
		/// <param name="args">pathToGame:string = Path to the game directory </param>
		static void Main(string[] args)
		{
			_pathToGame = args[0];
			if (string.IsNullOrEmpty(_pathToGame))
			{
				Console.WriteLine("Path to game is null or empty");
				return ;
			}
			if(!Directory.Exists(_pathToGame) )
			{
				Console.WriteLine("The directory {0} does not exist, can't reset leaderboars and tutorials",_pathToGame);
				return;
			}
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
			
			Console.WriteLine("Succesfully loaded CorePluggin");
			
			CleanResource.ChangeTutorialCompleted(_pathToGame);
			CleanResource.ClearLeaderboardsAndAchivements(_pathToGame);

			Console.WriteLine("Finished reseting resources");
			
		}

		private static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
		{
			const string coreDll = "gameplugin.core";
			var pluginPath = Path.Combine(_pathToGame, "plugins", coreDll + ".dll");
			if (!File.Exists(pluginPath))
			{
				Console.WriteLine("Can't find Core Plugin, abort resource clean up");
				throw new ArgumentException(string.Format("{0} not found, cant reset resources",pluginPath));
			}

			if (args.RequestingAssembly.FullName.ToLower().Contains(coreDll))
			{
				return Assembly.LoadFile(pluginPath);
			}
			return null;

		}


	}

	public class CleanResource
	{
		public static void ChangeTutorialCompleted(string pathToGame)
		{
			var playerProgressionResource = GameRes.Data.SaveInformation.PlayerProgressionResource_PlayerProgressionResource;
			var content = LoadResource<PlayerProgressionResource>(pathToGame, playerProgressionResource.Path);
			if (content == null)
				return;
			Console.WriteLine("Loaded player progression resource");
			content.Res.TutorialCompleted = false;
			content.Res.Save();
			Console.WriteLine("Completed reseting to tutorial to not completed");
		}

		public static void ClearLeaderboardsAndAchivements(string pathToGame)
		{
			var leaderboard = GameRes.Data.Platform.LocalPlatformResource_LocalPlatformResource;
			var content = LoadResource<LocalPlatformResource>(pathToGame, leaderboard.Path);
			if (content == null) 
				return;
			Console.WriteLine("Loaded platform resource");

			content.Res.LeaderboardInfo.Clear();
			content.Res.AchivementsUnlocked.Clear();
			content.Res.Save();
			Console.WriteLine("Completed reseting local leaderboards and achivements");

		}

		private static ContentRef<T> LoadResource<T>(string pathToGame,string pathToResource) where T : Resource
		{
			if (string.IsNullOrEmpty(pathToGame))
			{
				Console.WriteLine("Path to game is null or empty");
				return null;
			}
			if (string.IsNullOrEmpty(pathToResource))
			{
				Console.WriteLine("Path to resource is null or empty");
				return null;
			}
			var resourcePath = Path.Combine(pathToGame, pathToResource);
			if (!File.Exists(resourcePath))
			{
				Console.WriteLine("The resource {0} can't be found in {1}", Path.GetFileName(resourcePath), resourcePath);
				return null;
			}

			var content = new ContentRef<T>(null, resourcePath);
			if (content == null || content.Res == ContentRef<T>.Null)
			{
				Console.WriteLine("Leaderboard resource was null");
				return null;
			}
			return content;
		}
	}
}
