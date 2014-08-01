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
		/// <summary>
		/// Resets the local leaderboard and achievement as well as 
		/// </summary>
		/// <param name="args">pathToGame:string = Path to the game directory </param>
		static void Main(string[] args)
		{
			string pathToGame = args[0];
			if (string.IsNullOrEmpty(pathToGame))
			{
				Console.WriteLine("Path to game is null or empty");
				return ;
			}
			if(!Directory.Exists(pathToGame) )
			{
				Console.WriteLine("The directory {0} does not exist, can't reset leaderboars and tutorials",pathToGame);
				return;
			}
			CleanResource.ChangeTutorialCompleted(pathToGame);
			CleanResource.ClearLeaderboardsAndAchivements(pathToGame);
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
			content.Res.TutorialCompleted = false;
			content.Res.Save();
		}

		public static void ClearLeaderboardsAndAchivements(string pathToGame)
		{
			var leaderboard = GameRes.Data.Platform.LocalPlatformResource_LocalPlatformResource;
			var content = LoadResource<LocalPlatformResource>(pathToGame, leaderboard.Path);
			if (content == null) 
				return;
			content.Res.LeaderboardInfo.Clear();
			content.Res.AchivementsUnlocked.Clear();
			content.Res.Save();
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
