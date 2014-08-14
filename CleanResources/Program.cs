using System;
using System.IO;
using System.Reflection;

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
			
			ResetResources.ChangeTutorialCompleted(_pathToGame);
			ResetResources.ClearLeaderboardsAndAchivements(_pathToGame);

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

			return args.Name.ToLower().Contains(coreDll) ? Assembly.LoadFile(pluginPath) : null;
		}


	}
}
